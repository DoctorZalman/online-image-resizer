using ImageResizer.Hubs;
using ImageResizer.Models;
using Microsoft.AspNetCore.SignalR;

namespace ImageResizer.Services;

// - background worker that drains per-user queues in parallel
public class ResizeWorker : BackgroundService
{
    private readonly UserQueueManager _queueManager;
    private readonly ImageResizeService _resizeService;
    private readonly IHubContext<ResizeHub> _hub;
    private readonly ILogger<ResizeWorker> _logger;

    public ResizeWorker(
        UserQueueManager queueManager,
        ImageResizeService resizeService,
        IHubContext<ResizeHub> hub,
        ILogger<ResizeWorker> logger
    )
    {
        _queueManager = queueManager;
        _resizeService = resizeService;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var queues = _queueManager.GetAllQueues().ToList();

            if (queues.Count == 0)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }

            // - process all user queues in parallel, each sequentially within its own queue
            await Task.WhenAll(queues.Select(kv => ProcessQueueAsync(kv.Key, stoppingToken)));

            await Task.Delay(200, stoppingToken);
        }
    }

    // - drain one user's queue sequentially
    private async Task ProcessQueueAsync(string sessionId, CancellationToken ct)
    {
        var channel = _queueManager.GetOrCreateQueue(sessionId);

        while (channel.Reader.TryRead(out var job))
        {
            await ProcessJobAsync(job, ct);
        }
    }

    private async Task ProcessJobAsync(ResizeJob job, CancellationToken ct)
    {
        var group = _hub.Clients.Group(job.SessionId);

        try
        {
            job.Status = JobStatus.Processing;
            await group.SendAsync(
                "ResizeProgress",
                new
                {
                    job.JobId,
                    Progress = 10,
                    Status = "processing",
                },
                ct
            );

            var progress = new Progress<int>(async pct =>
            {
                job.Progress = pct;
                await group.SendAsync(
                    "ResizeProgress",
                    new
                    {
                        job.JobId,
                        Progress = pct,
                        Status = "processing",
                    },
                    ct
                );
            });

            await _resizeService.ResizeAsync(
                job.SourcePath,
                job.TargetPath,
                job.ScalePercent,
                progress
            );

            job.Status = JobStatus.Done;
            var downloadUrl = $"/api/images/download/{job.JobId}";
            await group.SendAsync(
                "ResizeComplete",
                new { job.JobId, DownloadUrl = downloadUrl },
                ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process job {JobId}", job.JobId);
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            await group.SendAsync("ResizeFailed", new { job.JobId, Error = ex.Message }, ct);
        }
    }
}
