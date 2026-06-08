using ImageResizer.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ImageResizer.Tests;

public class CleanupServiceTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public CleanupServiceTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    // - create a file and set its last write time to simulate age
    private string CreateFileWithAge(string subDir, TimeSpan age)
    {
        var dir = Path.Combine(_tempDir, subDir);
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "test.png");
        File.WriteAllText(file, "test");
        File.SetLastWriteTimeUtc(file, DateTime.UtcNow - age);
        return file;
    }

    [Fact]
    public void Cleanup_RemovesExpiredSessionDirectory()
    {
        CreateFileWithAge("session-old", TimeSpan.FromMinutes(15));

        var service = new TestableCleanupService(_tempDir, NullLogger<CleanupService>.Instance);
        service.RunCleanup();

        Assert.False(Directory.Exists(Path.Combine(_tempDir, "session-old")));
    }

    [Fact]
    public void Cleanup_PreservesRecentSessionDirectory()
    {
        CreateFileWithAge("session-new", TimeSpan.FromMinutes(2));

        var service = new TestableCleanupService(_tempDir, NullLogger<CleanupService>.Instance);
        service.RunCleanup();

        Assert.True(Directory.Exists(Path.Combine(_tempDir, "session-new")));
    }

    [Fact]
    public void Cleanup_RemovesOnlyExpiredDirectories()
    {
        CreateFileWithAge("session-old", TimeSpan.FromMinutes(15));
        CreateFileWithAge("session-new", TimeSpan.FromMinutes(2));

        var service = new TestableCleanupService(_tempDir, NullLogger<CleanupService>.Instance);
        service.RunCleanup();

        Assert.False(Directory.Exists(Path.Combine(_tempDir, "session-old")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "session-new")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
