namespace ImageResizer.Models;

public enum JobStatus
{
    Queued,
    Processing,
    Done,
    Failed,
}

// - represents a single image resize task in the processing pipeline
public class ResizeJob
{
    public string JobId { get; init; } = Guid.NewGuid().ToString();
    public string SessionId { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string TargetPath { get; init; } = string.Empty;
    public int ScalePercent { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;

    // - progress 0-100 reported via SignalR
    public int Progress { get; set; }
    public string? ErrorMessage { get; set; }
}
