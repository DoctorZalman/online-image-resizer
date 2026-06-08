using ImageResizer.Services;
using Microsoft.Extensions.Logging;

namespace ImageResizer.Tests;

// - exposes protected cleanup logic for unit testing without running background service
public class TestableCleanupService : CleanupService
{
    private readonly string _basePath;

    public TestableCleanupService(string basePath, ILogger<CleanupService> logger)
        : base(logger)
    {
        _basePath = basePath;
    }

    public void RunCleanup() => CleanupDirectory(_basePath);
}
