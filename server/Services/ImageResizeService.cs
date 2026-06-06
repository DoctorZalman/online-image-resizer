using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageResizer.Services;

public class ImageResizeService
{
    // - resize image file by scale percent, save to targetPath
    public async Task ResizeAsync(
        string sourcePath,
        string targetPath,
        int scalePercent,
        IProgress<int>? progress = null
    )
    {
        using var image = await Image.LoadAsync(sourcePath);

        var newWidth = (int)Math.Round(image.Width * scalePercent / 100.0);
        var newHeight = (int)Math.Round(image.Height * scalePercent / 100.0);

        progress?.Report(30);

        image.Mutate(x => x.Resize(newWidth, newHeight));

        progress?.Report(70);

        // - ensure target directory exists before saving
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        await image.SaveAsync(targetPath);

        progress?.Report(100);
    }
}
