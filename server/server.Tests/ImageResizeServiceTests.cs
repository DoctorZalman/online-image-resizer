using ImageResizer.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace ImageResizer.Tests;

public class ImageResizeServiceTests : IDisposable
{
    private readonly ImageResizeService _service = new();
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public ImageResizeServiceTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    // - create a test image file of given dimensions
    private async Task<string> CreateTestImageAsync(int width, int height)
    {
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid()}.png");
        using var image = new Image<Rgba32>(width, height);
        await image.SaveAsPngAsync(path);
        return path;
    }

    [Theory]
    [InlineData(100, 100, 50)]
    [InlineData(200, 150, 75)]
    [InlineData(400, 300, 25)]
    public async Task ResizeAsync_ScalesImageCorrectly(int width, int height, int scalePercent)
    {
        var source = await CreateTestImageAsync(width, height);
        var target = Path.Combine(_tempDir, $"{Guid.NewGuid()}.png");

        await _service.ResizeAsync(source, target, scalePercent);

        using var result = await Image.LoadAsync(target);
        var expectedWidth = (int)Math.Round(width * scalePercent / 100.0);
        var expectedHeight = (int)Math.Round(height * scalePercent / 100.0);

        Assert.Equal(expectedWidth, result.Width);
        Assert.Equal(expectedHeight, result.Height);
    }

    [Fact]
    public async Task ResizeAsync_ReportsProgress()
    {
        var source = await CreateTestImageAsync(100, 100);
        var target = Path.Combine(_tempDir, $"{Guid.NewGuid()}.png");
        var reported = new List<int>();

        var progress = new Progress<int>(p => reported.Add(p));
        await _service.ResizeAsync(source, target, 50, progress);

        // - small delay to allow Progress<T> callbacks to complete
        await Task.Delay(50);
        Assert.NotEmpty(reported);
        Assert.Contains(100, reported);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
