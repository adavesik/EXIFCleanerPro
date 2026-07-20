namespace EXIFCleanerPro.Services;

internal static class MetadataDisplayPolicy
{
    public static bool IsPrivacyNeutralGroup(string group) =>
        group.Contains("ICC Profile", StringComparison.OrdinalIgnoreCase);

    public static bool IsAdvancedOnly(string group, string tag, string value)
    {
        if (IsPrivacyNeutralGroup(group))
        {
            return true;
        }

        if (value.Length > 240)
        {
            return true;
        }

        return ContainsAny(tag, "TRC", "tone reproduction curve", "Huffman", "quantization table");
    }

    private static bool ContainsAny(string value, params string[] candidates) =>
        candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));
}
