using EXIFCleanerPro.Services;
using System.Drawing;
using System.Drawing.Imaging;
using ImageMetadataReader = MetadataExtractor.ImageMetadataReader;

namespace EXIFCleanerPro.Tests;

public sealed class ImageCleaningServiceTests : IDisposable
{
    private readonly string testDirectory = Path.Combine(Path.GetTempPath(), $"ExifCleanerImages-{Guid.NewGuid():N}");

    public ImageCleaningServiceTests() => Directory.CreateDirectory(testDirectory);

    [Fact]
    public async Task CleanedCopyLeavesOriginalUnchangedAndRemovesInjectedMetadata()
    {
        string input = CreateJpegWithDescription("photo.jpg");
        byte[] originalBytes = await File.ReadAllBytesAsync(input);
        Assert.Contains(
            ImageMetadataReader.ReadMetadata(input).SelectMany(directory => directory.Tags),
            tag => tag.Name.Contains("Description", StringComparison.OrdinalIgnoreCase));
        ImageCleaningService service = new();

        CleaningResult result = await service.CleanAsync(
            input,
            new CleaningOptions(OutputMode.CleanedCopy, null),
            CancellationToken.None);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.OutputPath);
        Assert.True(File.Exists(result.OutputPath));
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(input));
        Assert.DoesNotContain(
            ImageMetadataReader.ReadMetadata(result.OutputPath).SelectMany(directory => directory.Tags),
            tag => tag.Name.Contains("Description", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ReplaceModeCreatesBackupOfOriginal()
    {
        string input = CreateJpegWithDescription("replace.jpg");
        byte[] originalBytes = await File.ReadAllBytesAsync(input);
        ImageCleaningService service = new();

        CleaningResult result = await service.CleanAsync(
            input,
            new CleaningOptions(OutputMode.ReplaceWithBackup, null),
            CancellationToken.None);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.BackupPath);
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(result.BackupPath));
        Assert.DoesNotContain(
            ImageMetadataReader.ReadMetadata(input).SelectMany(directory => directory.Tags),
            tag => tag.Name.Contains("Description", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task PreCancelledOperationDoesNotCreateOutput()
    {
        string input = CreatePng("cancel.png");
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        ImageCleaningService service = new();

        CleaningResult result = await service.CleanAsync(
            input,
            new CleaningOptions(OutputMode.CleanedCopy, null),
            cancellation.Token);

        Assert.True(result.Cancelled);
        Assert.False(File.Exists(Path.Combine(testDirectory, "cancel-cleaned.png")));
    }

    [Fact]
    public async Task CleanedOutputPassesSensitiveMetadataVerification()
    {
        string input = CreateJpegWithDescription("verified.jpg");
        MetadataService metadataService = new();
        MetadataResult before = await metadataService.ReadAsync(input, CancellationToken.None);
        Assert.Contains(before.Entries, entry => entry.IsSensitive && entry.Tag.Contains("Description", StringComparison.OrdinalIgnoreCase));
        ImageCleaningService cleaningService = new();

        CleaningResult cleaning = await cleaningService.CleanAsync(
            input,
            new CleaningOptions(OutputMode.CleanedCopy, null),
            CancellationToken.None);
        MetadataResult after = await metadataService.ReadAsync(cleaning.OutputPath!, CancellationToken.None);
        MetadataComparison comparison = MetadataPrivacyAnalyzer.Compare(before, after);

        Assert.True(comparison.VerificationPassed, comparison.SensitiveCountSummary);
        Assert.True(comparison.RemovedSensitiveEntryCount > 0);
    }

    public void Dispose() => Directory.Delete(testDirectory, true);

    private string CreatePng(string name)
    {
        string path = Path.Combine(testDirectory, name);
        using Bitmap bitmap = new(12, 12);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.CadetBlue);
        bitmap.Save(path, ImageFormat.Png);
        return path;
    }

    private string CreateJpegWithDescription(string name)
    {
        string path = Path.Combine(testDirectory, name);
        using (Bitmap bitmap = new(12, 12))
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.CadetBlue);
            bitmap.Save(path, ImageFormat.Jpeg);
        }

        byte[] jpeg = File.ReadAllBytes(path);
        byte[] description = "hello\0"u8.ToArray();
        byte[] tiff = BuildMinimalExifTiff(description);
        byte[] payload = "Exif\0\0"u8.ToArray().Concat(tiff).ToArray();
        int segmentLength = payload.Length + 2;
        byte[] app1 = [0xFF, 0xE1, (byte)(segmentLength >> 8), (byte)segmentLength, .. payload];
        byte[] withExif = [jpeg[0], jpeg[1], .. app1, .. jpeg.AsSpan(2).ToArray()];
        File.WriteAllBytes(path, withExif);
        return path;
    }

    private static byte[] BuildMinimalExifTiff(byte[] description)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write((byte)'I');
        writer.Write((byte)'I');
        writer.Write((ushort)42);
        writer.Write(8u);
        writer.Write((ushort)1);
        writer.Write((ushort)0x010E);
        writer.Write((ushort)2);
        writer.Write((uint)description.Length);
        writer.Write(26u);
        writer.Write(0u);
        writer.Write(description);
        return stream.ToArray();
    }
}
