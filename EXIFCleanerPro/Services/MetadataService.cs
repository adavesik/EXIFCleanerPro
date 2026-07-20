using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Globalization;

namespace EXIFCleanerPro.Services;

internal sealed class MetadataService : IMetadataService
{
    public Task<MetadataResult> ReadAsync(string filePath, CancellationToken cancellationToken) =>
        Task.Run(() => Read(filePath, cancellationToken), cancellationToken);

    private static MetadataResult Read(string filePath, CancellationToken cancellationToken)
    {
        var directories = ImageMetadataReader.ReadMetadata(filePath);
        List<MetadataEntry> entries = [];
        bool hasCamera = false;
        bool hasDate = false;
        bool hasSoftware = false;

        foreach (var directory in directories)
        {
            cancellationToken.ThrowIfCancellationRequested();
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
        double? latitude = null;
        double? longitude = null;
        if (location is GeoLocation coordinates &&
            double.IsFinite(coordinates.Latitude) &&
            double.IsFinite(coordinates.Longitude) &&
            coordinates.Latitude is >= -90 and <= 90 &&
            coordinates.Longitude is >= -180 and <= 180)
        {
            latitude = coordinates.Latitude;
            longitude = coordinates.Longitude;
            string coordinateValue = $"{coordinates.Latitude.ToString("0.######", CultureInfo.InvariantCulture)}, {coordinates.Longitude.ToString("0.######", CultureInfo.InvariantCulture)}";
            entries.Insert(0, new MetadataEntry("GPS", "GPS Coordinates", coordinateValue));
        }

        MetadataInterpretation interpretation = MetadataPrivacyAnalyzer.Interpret(entries);
        bool hasGps = interpretation.Entries.Any(entry => entry.PrivacyCategory == PrivacyCategory.Location);
        return new MetadataResult(
            interpretation.Entries,
            hasGps,
            hasCamera,
            hasDate,
            hasSoftware,
            interpretation.Assessment,
            latitude,
            longitude);
    }
}
