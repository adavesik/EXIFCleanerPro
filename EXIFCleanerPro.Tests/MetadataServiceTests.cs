using EXIFCleanerPro.Services;
using System.Drawing;
using System.Drawing.Imaging;

namespace EXIFCleanerPro.Tests;

public sealed class MetadataServiceTests : IDisposable
{
    private readonly string testDirectory = Path.Combine(Path.GetTempPath(), $"ExifCleanerGps-{Guid.NewGuid():N}");

    public MetadataServiceTests() => Directory.CreateDirectory(testDirectory);

    [Fact]
    public async Task ReadAsyncAddsVisibleCoordinateSummaryForValidGpsData()
    {
        string imagePath = CreateJpegWithGps("location.jpg", includeCoordinates: true);
        MetadataService service = new();

        MetadataResult result = await service.ReadAsync(imagePath, CancellationToken.None);

        Assert.True(result.HasGps);
        Assert.NotNull(result.MapUri);
        Assert.Equal(40.4168, result.Latitude!.Value, 4);
        Assert.Equal(-3.7038, result.Longitude!.Value, 4);
        MetadataEntry summary = Assert.Single(result.Entries, entry => entry.Tag == "GPS Coordinates");
        Assert.Equal("GPS coordinates", summary.DisplayName);
        Assert.Equal("40.4168, -3.7038", summary.DisplayValue);
        Assert.True(summary.IsSensitive);
    }

    [Fact]
    public async Task ReadAsyncDoesNotClaimLocationForGpsVersionOnly()
    {
        string imagePath = CreateJpegWithGps("version-only.jpg", includeCoordinates: false);
        MetadataService service = new();

        MetadataResult result = await service.ReadAsync(imagePath, CancellationToken.None);

        Assert.False(result.HasGps);
        Assert.Null(result.MapUri);
        Assert.DoesNotContain(result.Assessment.Findings, finding => finding.Category == PrivacyCategory.Location);
    }

    public void Dispose() => Directory.Delete(testDirectory, true);

    private string CreateJpegWithGps(string name, bool includeCoordinates)
    {
        string path = Path.Combine(testDirectory, name);
        using (Bitmap bitmap = new(12, 12))
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.CadetBlue);
            bitmap.Save(path, ImageFormat.Jpeg);
        }

        byte[] jpeg = File.ReadAllBytes(path);
        byte[] tiff = BuildGpsTiff(includeCoordinates);
        byte[] payload = "Exif\0\0"u8.ToArray().Concat(tiff).ToArray();
        int segmentLength = payload.Length + 2;
        byte[] app1 = [0xFF, 0xE1, (byte)(segmentLength >> 8), (byte)segmentLength, .. payload];
        File.WriteAllBytes(path, [jpeg[0], jpeg[1], .. app1, .. jpeg.AsSpan(2).ToArray()]);
        return path;
    }

    private static byte[] BuildGpsTiff(bool includeCoordinates)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write((byte)'I');
        writer.Write((byte)'I');
        writer.Write((ushort)42);
        writer.Write(8u);

        writer.Write((ushort)1);
        WriteIfdEntry(writer, 0x8825, 4, 1, 26);
        writer.Write(0u);

        ushort gpsEntryCount = includeCoordinates ? (ushort)5 : (ushort)1;
        writer.Write(gpsEntryCount);
        WriteInlineBytesEntry(writer, 0x0000, 1, 4, [2, 3, 0, 0]);
        if (!includeCoordinates)
        {
            writer.Write(0u);
            return stream.ToArray();
        }

        WriteInlineBytesEntry(writer, 0x0001, 2, 2, [(byte)'N', 0, 0, 0]);
        const uint coordinateDataOffset = 92;
        WriteIfdEntry(writer, 0x0002, 5, 3, coordinateDataOffset);
        WriteInlineBytesEntry(writer, 0x0003, 2, 2, [(byte)'W', 0, 0, 0]);
        WriteIfdEntry(writer, 0x0004, 5, 3, coordinateDataOffset + 24);
        writer.Write(0u);
        WriteCoordinate(writer, 40.4168);
        WriteCoordinate(writer, 3.7038);
        return stream.ToArray();
    }

    private static void WriteIfdEntry(BinaryWriter writer, ushort tag, ushort type, uint count, uint value)
    {
        writer.Write(tag);
        writer.Write(type);
        writer.Write(count);
        writer.Write(value);
    }

    private static void WriteInlineBytesEntry(BinaryWriter writer, ushort tag, ushort type, uint count, byte[] value)
    {
        writer.Write(tag);
        writer.Write(type);
        writer.Write(count);
        writer.Write(value);
    }

    private static void WriteCoordinate(BinaryWriter writer, double coordinate)
    {
        uint degrees = (uint)Math.Floor(coordinate);
        double minutesValue = (coordinate - degrees) * 60;
        uint minutes = (uint)Math.Floor(minutesValue);
        uint seconds = (uint)Math.Round((minutesValue - minutes) * 60 * 10_000);
        writer.Write(degrees);
        writer.Write(1u);
        writer.Write(minutes);
        writer.Write(1u);
        writer.Write(seconds);
        writer.Write(10_000u);
    }
}
