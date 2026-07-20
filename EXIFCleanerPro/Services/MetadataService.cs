using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace EXIFCleanerPro.Services;

internal sealed class MetadataService : IMetadataService
{
    public Task<MetadataResult> ReadAsync(string filePath, CancellationToken cancellationToken) =>
        Task.Run(() => Read(filePath, cancellationToken), cancellationToken);

    private static MetadataResult Read(string filePath, CancellationToken cancellationToken)
    {
        var directories = ImageMetadataReader.ReadMetadata(filePath);
        List<MetadataEntry> entries = [];
        bool hasGps = false;
        bool hasCamera = false;
        bool hasDate = false;
        bool hasSoftware = false;

        foreach (var directory in directories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            hasGps |= directory.Name.Contains("GPS", StringComparison.OrdinalIgnoreCase);

            foreach (var tag in directory.Tags)
            {
                string tagName = tag.Name;
                string value = tag.Description ?? string.Empty;
                entries.Add(new MetadataEntry(directory.Name, tagName, value));

                hasCamera |= tagName.Contains("Make", StringComparison.OrdinalIgnoreCase) ||
                             tagName.Contains("Model", StringComparison.OrdinalIgnoreCase) ||
                             tagName.Contains("Lens", StringComparison.OrdinalIgnoreCase);
                hasDate |= tagName.Contains("Date", StringComparison.OrdinalIgnoreCase) ||
                           tagName.Contains("Time", StringComparison.OrdinalIgnoreCase);
                hasSoftware |= tagName.Contains("Software", StringComparison.OrdinalIgnoreCase) ||
                               tagName.Contains("Creator", StringComparison.OrdinalIgnoreCase);
            }
        }

        GpsDirectory? gpsDirectory = directories.OfType<GpsDirectory>().FirstOrDefault();
        GeoLocation? location = gpsDirectory?.GetGeoLocation();
        MetadataInterpretation interpretation = MetadataPrivacyAnalyzer.Interpret(entries);
        return new MetadataResult(
            interpretation.Entries,
            hasGps,
            hasCamera,
            hasDate,
            hasSoftware,
            interpretation.Assessment,
            location?.Latitude,
            location?.Longitude);
    }
}
