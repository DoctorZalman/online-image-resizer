namespace ImageResizer.Services;

// - hosted background service that removes expired temp files every 5 minutes
public class CleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(10);
    private readonly string _basePath = Path.Combine(Path.GetTempPath(), "image-resizer");
    private readonly ILogger<CleanupService> _logger;

    public CleanupService(ILogger<CleanupService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);
            Cleanup();
        }
    }

    // - delete session folders whose files are older than MaxAge
    private void Cleanup()
    {
        if (!Directory.Exists(_basePath))
            return;

        foreach (var sessionDir in Directory.GetDirectories(_basePath))
        {
            try
            {
                var files = Directory.GetFiles(sessionDir, "*", SearchOption.AllDirectories);
                var allExpired = files.All(f =>
                    DateTime.UtcNow - File.GetLastWriteTimeUtc(f) > MaxAge
                );

                if (allExpired)
                {
                    Directory.Delete(sessionDir, recursive: true);
                    _logger.LogInformation("Cleaned up session dir: {Dir}", sessionDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up directory: {Dir}", sessionDir);
            }
        }
    }
}
