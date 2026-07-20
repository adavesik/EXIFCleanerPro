using System.Globalization;

namespace EXIFCleanerPro;

internal sealed record MetadataEntry(
    string Group,
    string Tag,
    string Value,
    string? FriendlyName = null,
    string? Explanation = null,
    PrivacyCategory? PrivacyCategory = null,
    bool IsSensitive = false)
{
    public string DisplayName => FriendlyName ?? Tag;
    public string DisplayExplanation => Explanation ?? $"Technical metadata stored as {Tag}.";
    public string ClipboardText => $"{Group} — {DisplayName}: {Value}";
}

internal sealed record MetadataResult(
    IReadOnlyList<MetadataEntry> Entries,
    bool HasGps,
    bool HasCamera,
    bool HasDate,
    bool HasSoftware,
    PrivacyAssessment Assessment,
    double? Latitude = null,
    double? Longitude = null)
{
    public MetadataResult(
        IReadOnlyList<MetadataEntry> entries,
        bool hasGps,
        bool hasCamera,
        bool hasDate,
        bool hasSoftware)
        : this(entries, hasGps, hasCamera, hasDate, hasSoftware, PrivacyAssessment.Empty)
    {
    }

    public static MetadataResult Empty { get; } = new([], false, false, false, false);

    public Uri? MapUri => Latitude is double latitude && Longitude is double longitude
        ? new Uri(
            $"https://www.openstreetmap.org/?mlat={latitude.ToString(CultureInfo.InvariantCulture)}&mlon={longitude.ToString(CultureInfo.InvariantCulture)}#map=16/{latitude.ToString(CultureInfo.InvariantCulture)}/{longitude.ToString(CultureInfo.InvariantCulture)}")
        : null;
}

internal sealed record PrivacyFinding(
    string Key,
    string Title,
    string Explanation,
    PrivacyCategory Category,
    int Points,
    int MatchingEntryCount);

internal sealed record PrivacyCategoryRisk(
    PrivacyCategory Category,
    int Score,
    PrivacyRiskLevel Level);

internal sealed record PrivacyAssessment(
    int Score,
    PrivacyRiskLevel Level,
    IReadOnlyList<PrivacyFinding> Findings,
    IReadOnlyList<PrivacyCategoryRisk> Categories)
{
    public static PrivacyAssessment Empty { get; } = new(0, PrivacyRiskLevel.None, [], []);
    public string Summary => Score == 0 ? "No sensitive metadata detected" : $"{Level} risk · {Score}/100";
}

internal sealed record MetadataComparison(
    int BeforeEntryCount,
    int AfterEntryCount,
    int BeforeSensitiveEntryCount,
    int AfterSensitiveEntryCount,
    int RemovedSensitiveEntryCount,
    IReadOnlyList<PrivacyFinding> RemainingFindings)
{
    public bool VerificationPassed => RemainingFindings.Count == 0;
    public string EntryCountSummary => $"{BeforeEntryCount} → {AfterEntryCount} metadata entries";
    public string SensitiveCountSummary => VerificationPassed
        ? $"Verified: {RemovedSensitiveEntryCount} sensitive entries removed"
        : $"Warning: {AfterSensitiveEntryCount} sensitive entries remain";
}

internal enum PrivacyCategory
{
    Location,
    Identity,
    Device,
    Timeline,
    EditingHistory,
    EmbeddedText
}

internal enum PrivacyRiskLevel
{
    None,
    Low,
    Medium,
    High,
    Critical
}
