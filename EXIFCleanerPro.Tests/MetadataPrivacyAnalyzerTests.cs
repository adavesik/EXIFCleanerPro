using EXIFCleanerPro.Services;

namespace EXIFCleanerPro.Tests;

public sealed class MetadataPrivacyAnalyzerTests
{
    [Fact]
    public void InterpretCreatesFriendlyFindingsAndDeterministicScore()
    {
        MetadataEntry[] raw =
        [
            new("GPS", "GPS Latitude", "40.4168"),
            new("Exif IFD0", "Camera Owner Name", "Jane Doe"),
            new("Exif SubIFD", "Body Serial Number", "ABC123"),
            new("Exif SubIFD", "Date/Time Original", "2026:07:20 12:30:00"),
            new("Exif IFD0", "Software", "Photo Editor")
        ];

        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(raw);

        Assert.Equal(100, result.Assessment.Score);
        Assert.Equal(PrivacyRiskLevel.Critical, result.Assessment.Level);
        Assert.Equal(5, result.Assessment.Findings.Count);
        Assert.All(result.Entries, entry => Assert.True(entry.IsSensitive));
        Assert.Equal("GPS location", result.Entries[0].DisplayName);
        Assert.Contains(result.Assessment.Categories, category => category.Category == PrivacyCategory.Location);
    }

    [Fact]
    public void ComparePassesWhenOnlyStructuralMetadataRemains()
    {
        MetadataResult before = CreateResult(
            new MetadataEntry("GPS", "GPS Longitude", "-3.7038"),
            new MetadataEntry("JPEG", "Image Width", "1200 pixels"));
        MetadataResult after = CreateResult(
            new MetadataEntry("JPEG", "Image Width", "1200 pixels"),
            new MetadataEntry("JPEG", "Compression Type", "Baseline"));

        MetadataComparison comparison = MetadataPrivacyAnalyzer.Compare(before, after);

        Assert.True(comparison.VerificationPassed);
        Assert.Equal(1, comparison.RemovedSensitiveEntryCount);
        Assert.Equal(0, comparison.AfterSensitiveEntryCount);
    }

    [Fact]
    public void CompareFailsWhenSensitiveFindingRemains()
    {
        MetadataResult before = CreateResult(new MetadataEntry("Exif", "Software", "Editor"));
        MetadataResult after = CreateResult(new MetadataEntry("XMP", "Software", "Editor"));

        MetadataComparison comparison = MetadataPrivacyAnalyzer.Compare(before, after);

        Assert.False(comparison.VerificationPassed);
        Assert.Single(comparison.RemainingFindings);
    }

    [Fact]
    public void ExposureTimeAndFileModifiedDateAreNotPrivacyTimelineFindings()
    {
        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(
        [
            new MetadataEntry("Exif SubIFD", "Exposure Time", "1/250 sec"),
            new MetadataEntry("File", "File Modified Date", "2026-07-20")
        ]);

        Assert.Equal(0, result.Assessment.Score);
        Assert.Empty(result.Assessment.Findings);
    }

    private static MetadataResult CreateResult(params MetadataEntry[] raw)
    {
        MetadataInterpretation interpretation = MetadataPrivacyAnalyzer.Interpret(raw);
        return new MetadataResult(
            interpretation.Entries,
            raw.Any(entry => entry.Group.Contains("GPS", StringComparison.OrdinalIgnoreCase)),
            false,
            false,
            false,
            interpretation.Assessment);
    }
}
