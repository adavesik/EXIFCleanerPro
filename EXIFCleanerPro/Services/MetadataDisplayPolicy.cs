namespace EXIFCleanerPro.Services;

internal static class MetadataDisplayPolicy
{
    public static bool IsAdvancedOnly(string group, string tag, string value)
    {
        if (group.Contains("ICC Profile", StringComparison.OrdinalIgnoreCase))
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
