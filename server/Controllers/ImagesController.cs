using ImageResizer.Models;
using ImageResizer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageResizer.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png"];
    private const int MaxFilesPerSession = 10;
    private readonly string _basePath = Path.Combine(Path.GetTempPath(), "image-resizer");

    private readonly UserQueueManager _queueManager;
    private readonly ILogger<ImagesController> _logger;

    // - in-memory job registry shared across requests
    private static readonly Dictionary<string, ResizeJob> Jobs = new();
    private static readonly Lock JobsLock = new();

    public ImagesController(UserQueueManager queueManager, ILogger<ImagesController> logger)
    {
        _queueManager = queueManager;
        _logger = logger;
    }

    // - accepts multipart/form-data with multiple image files
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        var sessionId = HttpContext.Items["SessionId"]?.ToString();
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized();

        if (files.Count == 0)
            return BadRequest("No files provided");
        if (files.Count > MaxFilesPerSession)
            return BadRequest($"Max {MaxFilesPerSession} files per session");

        var invalid = files.Where(f => !AllowedContentTypes.Contains(f.ContentType)).ToList();
        if (invalid.Count > 0)
            return BadRequest("Only JPEG/PNG files are allowed");

        var result = new List<object>();

        foreach (var file in files)
        {
            var jobId = Guid.NewGuid().ToString();
            var sessionDir = Path.Combine(_basePath, sessionId, jobId);
            Directory.CreateDirectory(sessionDir);

            var ext = Path.GetExtension(file.FileName);
            var sourcePath = Path.Combine(sessionDir, $"source{ext}");

            await using var stream = System.IO.File.Create(sourcePath);
            await file.CopyToAsync(stream);

            var job = new ResizeJob
            {
                JobId = jobId,
                SessionId = sessionId,
                SourcePath = sourcePath,
                TargetPath = Path.Combine(sessionDir, $"resized{ext}"),
            };

            lock (JobsLock)
                Jobs[jobId] = job;

            result.Add(
                new
                {
                    jobId,
                    fileName = file.FileName,
                    size = file.Length,
                }
            );
        }

        return Ok(result);
    }

    // - enqueue resize job, returns 202 so client does not wait for processing
    [HttpPost("resize")]
    public async Task<IActionResult> Resize([FromBody] ResizeRequest request)
    {
        var sessionId = HttpContext.Items["SessionId"]?.ToString();
        if (string.IsNullOrEmpty(sessionId))
            return Unauthorized();

        ResizeJob? job;
        lock (JobsLock)
            Jobs.TryGetValue(request.JobId, out job);

        if (job == null)
            return NotFound("Job not found");
        if (job.SessionId != sessionId)
            return Forbid();

        job.ScalePercent = request.ScalePercent;
        job.Status = JobStatus.Queued;

        await _queueManager.EnqueueAsync(sessionId, job);

        return Accepted(new { request.JobId });
    }

    // - stream resized file to client and delete after download
    [HttpGet("download/{jobId}")]
    public IActionResult Download(string jobId)
    {
        ResizeJob? job;
        lock (JobsLock)
            Jobs.TryGetValue(jobId, out job);

        if (job == null || job.Status != JobStatus.Done)
            return NotFound();
        if (!System.IO.File.Exists(job.TargetPath))
            return NotFound();

        var ext = Path.GetExtension(job.TargetPath);
        var contentType = ext == ".png" ? "image/png" : "image/jpeg";
        var fileName = $"resized_{jobId}{ext}";

        var stream = System.IO.File.OpenRead(job.TargetPath);

        // - delete job from registry after streaming
        Response.OnCompleted(() =>
        {
            stream.Dispose();
            try
            {
                System.IO.File.Delete(job.TargetPath);
            }
            catch { }
            lock (JobsLock)
                Jobs.Remove(jobId);
            return Task.CompletedTask;
        });

        return File(stream, contentType, fileName);
    }

    // - fallback status endpoint for clients without SignalR
    [HttpGet("status/{jobId}")]
    public IActionResult Status(string jobId)
    {
        ResizeJob? job;
        lock (JobsLock)
            Jobs.TryGetValue(jobId, out job);

        if (job == null)
            return NotFound();

        return Ok(
            new
            {
                job.JobId,
                job.Status,
                job.Progress,
                job.ErrorMessage,
            }
        );
    }
}

// - request body for resize endpoint
public record ResizeRequest(string JobId, int ScalePercent);
