namespace EXIFCleanerPro;

internal sealed record PrivacyReportData(
    string FileName,
    string FilePath,
    PrivacyAssessment Assessment,
    MetadataComparison? Comparison,
    DateTimeOffset GeneratedAt);
