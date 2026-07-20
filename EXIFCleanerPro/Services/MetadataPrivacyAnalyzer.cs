namespace EXIFCleanerPro.Services;

internal static class MetadataPrivacyAnalyzer
{
    private static readonly MetadataRule[] Rules =
    [
        new(
            "gps",
            "Precise location",
            "Coordinates can reveal exactly where this image was captured.",
            PrivacyCategory.Location,
            45,
            "GPS location",
            "This value contributes to the place where the image was captured.",
            IsLocationEntry),
        new(
            "device-id",
            "Device identifier",
            "A serial number or unique identifier can link images to a physical device.",
            PrivacyCategory.Device,
            20,
            "Device serial or identifier",
            "This value may identify the physical camera or image uniquely.",
            entry => ContainsAny(entry.Tag, "serial", "unique image id", "image unique id")),
        new(
            "identity",
            "Owner or author identity",
            "Creator, owner, artist, or copyright fields may identify a person or organization.",
            PrivacyCategory.Identity,
            25,
            "Owner or author",
            "This value may identify the image creator, owner, or rights holder.",
            entry => ContainsAny(entry.Tag, "artist", "author", "owner", "copyright", "credit", "contact", "by-line", "creator") &&
                     !Contains(entry.Tag, "creator tool")),
        new(
            "embedded-text",
            "Embedded text",
            "Descriptions, comments, captions, or keywords may contain names or other private context.",
            PrivacyCategory.EmbeddedText,
            15,
            "Embedded description or comment",
            "This free-text value may disclose names, context, or other private information.",
            entry => ContainsAny(entry.Tag, "description", "comment", "caption", "subject", "keyword")),
        new(
            "timeline",
            "Capture timeline",
            "Capture and modification times reveal when the image was created or processed.",
            PrivacyCategory.Timeline,
            10,
            "Capture or modification time",
            "This value reveals when the image was captured, digitized, or changed.",
            IsTimelineEntry),
        new(
            "editing",
            "Editing history",
            "Software and processing fields reveal tools or computers used to edit the image.",
            PrivacyCategory.EditingHistory,
            10,
            "Editing software or history",
            "This value can reveal software, processing steps, or a computer used for editing.",
            entry => ContainsAny(entry.Tag, "software", "history", "host computer", "processing", "creator tool")),
        new(
            "camera-profile",
            "Camera profile",
            "Camera and lens details can contribute to device fingerprinting.",
            PrivacyCategory.Device,
            5,
            "Camera or lens profile",
            "This technical value identifies the camera or lens used to create the image.",
            entry => ContainsAny(entry.Tag, "make", "model", "lens"))
    ];

    public static MetadataInterpretation Interpret(IReadOnlyList<MetadataEntry> source)
    {
        List<MetadataEntry> entries = new(source.Count);
        Dictionary<string, List<MetadataEntry>> matchingEntries = new(StringComparer.Ordinal);

        foreach (MetadataEntry entry in source)
        {
            MetadataRule? rule = Rules.FirstOrDefault(candidate => candidate.Matches(entry));
            if (rule is null)
            {
                entries.Add(entry);
                continue;
            }

            MetadataEntry interpreted = entry with
            {
                FriendlyName = GetFriendlyName(rule, entry),
                Explanation = GetEntryExplanation(rule, entry),
                PrivacyCategory = rule.Category,
                IsSensitive = true
            };
            entries.Add(interpreted);
            if (!matchingEntries.TryGetValue(rule.Key, out List<MetadataEntry>? matches))
            {
                matches = [];
                matchingEntries.Add(rule.Key, matches);
            }

            matches.Add(interpreted);
        }

        List<PrivacyFinding> findings = Rules
            .Where(rule => matchingEntries.ContainsKey(rule.Key))
            .Select(rule => new PrivacyFinding(
                rule.Key,
                rule.Title,
                rule.Explanation,
                rule.Category,
                rule.Points,
                matchingEntries[rule.Key].Count))
            .ToList();
        int score = Math.Min(100, findings.Sum(finding => finding.Points));
        List<PrivacyCategoryRisk> categories = findings
            .GroupBy(finding => finding.Category)
            .Select(group =>
            {
                int categoryScore = group.Sum(finding => finding.Points);
                return new PrivacyCategoryRisk(group.Key, categoryScore, GetRiskLevel(categoryScore));
            })
            .OrderByDescending(category => category.Score)
            .ToList();

        return new MetadataInterpretation(
            entries,
            new PrivacyAssessment(score, GetRiskLevel(score), findings, categories));
    }

    public static MetadataComparison Compare(MetadataResult before, MetadataResult after)
    {
        int beforeSensitive = before.Entries.Count(entry => entry.IsSensitive);
        int afterSensitive = after.Entries.Count(entry => entry.IsSensitive);
        return new MetadataComparison(
            before.Entries.Count,
            after.Entries.Count,
            beforeSensitive,
            afterSensitive,
            Math.Max(0, beforeSensitive - afterSensitive),
            after.Assessment.Findings);
    }

    internal static PrivacyRiskLevel GetRiskLevel(int score) => score switch
    {
        0 => PrivacyRiskLevel.None,
        <= 19 => PrivacyRiskLevel.Low,
        <= 44 => PrivacyRiskLevel.Medium,
        <= 69 => PrivacyRiskLevel.High,
        _ => PrivacyRiskLevel.Critical
    };

    private static bool Contains(string value, string candidate) =>
        value.Contains(candidate, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsAny(string value, params string[] candidates) =>
        candidates.Any(candidate => Contains(value, candidate));

    private static bool IsLocationEntry(MetadataEntry entry) =>
        !string.IsNullOrWhiteSpace(entry.Value) &&
        ContainsAny(entry.Tag, "gps coordinates", "latitude", "longitude", "altitude", "location");

    private static string GetFriendlyName(MetadataRule rule, MetadataEntry entry)
    {
        if (rule.Key != "gps")
        {
            return rule.FriendlyName;
        }

        return entry.Tag switch
        {
            string tag when Contains(tag, "coordinates") => "GPS coordinates",
            string tag when Contains(tag, "latitude") => "GPS latitude",
            string tag when Contains(tag, "longitude") => "GPS longitude",
            string tag when Contains(tag, "altitude") => "GPS altitude",
            _ => "GPS location"
        };
    }

    private static string GetEntryExplanation(MetadataRule rule, MetadataEntry entry)
    {
        if (rule.Key != "gps")
        {
            return rule.EntryExplanation;
        }

        return entry.Tag switch
        {
            string tag when Contains(tag, "coordinates") => "The combined latitude and longitude stored in this image.",
            string tag when Contains(tag, "latitude") => "The north-south coordinate stored in this image.",
            string tag when Contains(tag, "longitude") => "The east-west coordinate stored in this image.",
            string tag when Contains(tag, "altitude") => "The recorded height above or below sea level.",
            _ => rule.EntryExplanation
        };
    }

    private static bool IsTimelineEntry(MetadataEntry entry)
    {
        if (entry.Group.Equals("File", StringComparison.OrdinalIgnoreCase) ||
            entry.Group.Contains("File Type", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return entry.Tag.StartsWith("Date", StringComparison.OrdinalIgnoreCase) ||
               ContainsAny(
                   entry.Tag,
                   "date/time",
                   "datetime",
                   "create date",
                   "modify date",
                   "original date",
                   "digitized date",
                   "time original",
                   "offset time",
                   "timestamp");
    }

    private sealed record MetadataRule(
        string Key,
        string Title,
        string Explanation,
        PrivacyCategory Category,
        int Points,
        string FriendlyName,
        string EntryExplanation,
        Func<MetadataEntry, bool> Matches);
}

internal sealed record MetadataInterpretation(
    IReadOnlyList<MetadataEntry> Entries,
    PrivacyAssessment Assessment);
