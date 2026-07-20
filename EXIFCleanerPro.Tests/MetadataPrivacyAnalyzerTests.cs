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
        Assert.Equal("GPS latitude", result.Entries[0].DisplayName);
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

    [Fact]
    public void GpsVersionDirectoryDoesNotClaimPreciseLocation()
    {
        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(
        [
            new MetadataEntry("GPS", "GPS Version ID", "2.3.0.0")
        ]);

        Assert.Equal(0, result.Assessment.Score);
        Assert.Empty(result.Assessment.Findings);
        Assert.False(result.Entries[0].IsSensitive);
    }

    [Fact]
    public void GpsCoordinatePartsKeepDistinctFriendlyNamesAndValues()
    {
        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(
        [
            new MetadataEntry("GPS", "GPS Latitude", "40.4168"),
            new MetadataEntry("GPS", "GPS Longitude", "-3.7038")
        ]);

        Assert.Equal("GPS latitude", result.Entries[0].DisplayName);
        Assert.Equal("40.4168", result.Entries[0].DisplayValue);
        Assert.Equal("GPS longitude", result.Entries[1].DisplayName);
        Assert.Equal("-3.7038", result.Entries[1].DisplayValue);
        Assert.Single(result.Assessment.Findings);
    }

    [Fact]
    public void SearchMatchesHumanFriendlyMetadataName()
    {
        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(
        [
            new MetadataEntry("XMP", "Location", "Madrid")
        ]);

        MetadataEntry entry = Assert.Single(result.Entries);
        Assert.DoesNotContain("gps", entry.Tag, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("gps", entry.Group, StringComparison.OrdinalIgnoreCase);
        Assert.True(entry.MatchesSearch("gps"));
    }

    [Fact]
    public void EmptyLocationTagDoesNotTriggerGpsWarning()
    {
        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(
        [
            new MetadataEntry("XMP", "Location", string.Empty)
        ]);

        Assert.Equal(0, result.Assessment.Score);
        Assert.False(result.Entries[0].IsSensitive);
    }

    [Fact]
    public void MetadataCategoryFiltersMatchTheirInterpretedEntries()
    {
        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(
        [
            new MetadataEntry("GPS", "GPS Latitude", "40.4168"),
            new MetadataEntry("Exif IFD0", "Model", "Camera"),
            new MetadataEntry("Exif SubIFD", "Date/Time Original", "2026:07:20 12:30:00"),
            new MetadataEntry("Exif IFD0", "Software", "Editor")
        ]);

        Assert.Single(result.Entries, entry => entry.MatchesFilter(MetadataFilter.Location));
        Assert.Single(result.Entries, entry => entry.MatchesFilter(MetadataFilter.Camera));
        Assert.Single(result.Entries, entry => entry.MatchesFilter(MetadataFilter.Timeline));
        Assert.Single(result.Entries, entry => entry.MatchesFilter(MetadataFilter.Software));
    }

    [Fact]
    public void IccColorProfileFieldsDoNotCreatePrivacyRisk()
    {
        MetadataInterpretation result = MetadataPrivacyAnalyzer.Interpret(
        [
            new MetadataEntry("ICC Profile", "Profile Copyright", "Copyright ICC vendor", IsAdvancedOnly: true),
            new MetadataEntry("ICC Profile", "Profile Description", "sRGB profile", IsAdvancedOnly: true),
            new MetadataEntry("ICC Profile", "Profile Date/Time", "2026:07:20 12:30:00", IsAdvancedOnly: true),
            new MetadataEntry("ICC Profile", "Device Model", "Display profile", IsAdvancedOnly: true)
        ]);

        Assert.Equal(0, result.Assessment.Score);
        Assert.Empty(result.Assessment.Findings);
        Assert.All(result.Entries, entry => Assert.False(entry.IsSensitive));
    }

    [Fact]
    public void VerificationPassesWhenOnlyIccColorProfileFieldsRemain()
    {
        MetadataResult before = CreateResult(new MetadataEntry("Exif IFD0", "Software", "Editor"));
        MetadataResult after = CreateResult(
            new MetadataEntry("ICC Profile", "Profile Copyright", "Copyright ICC vendor", IsAdvancedOnly: true),
            new MetadataEntry("ICC Profile", "Profile Description", "sRGB profile", IsAdvancedOnly: true),
            new MetadataEntry("ICC Profile", "Device Model", "Display profile", IsAdvancedOnly: true));

        MetadataComparison comparison = MetadataPrivacyAnalyzer.Compare(before, after);

        Assert.True(comparison.VerificationPassed);
        Assert.Equal(0, comparison.AfterSensitiveEntryCount);
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
