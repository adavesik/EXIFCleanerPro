namespace EXIFCleanerPro.Services;

internal static class OutputPathResolver
{
    public static string GetCleanedCopyPath(string inputPath)
    {
        string directory = Path.GetDirectoryName(inputPath) ?? throw new InvalidOperationException("The input file has no directory.");
        string stem = Path.GetFileNameWithoutExtension(inputPath);
        string extension = Path.GetExtension(inputPath);
        return GetAvailablePath(Path.Combine(directory, $"{stem}-cleaned{extension}"));
    }

    public static string GetDestinationPath(string inputPath, string destinationFolder)
    {
        Directory.CreateDirectory(destinationFolder);
        return GetAvailablePath(Path.Combine(destinationFolder, Path.GetFileName(inputPath)));
    }

    public static string GetBackupPath(string inputPath)
    {
        string directory = Path.GetDirectoryName(inputPath) ?? throw new InvalidOperationException("The input file has no directory.");
        string stem = Path.GetFileNameWithoutExtension(inputPath);
        string extension = Path.GetExtension(inputPath);
        return GetAvailablePath(Path.Combine(directory, $"{stem}-backup{extension}"));
    }

    public static string GetAvailablePath(string desiredPath)
    {
        if (!File.Exists(desiredPath))
        {
            return desiredPath;
        }

        string directory = Path.GetDirectoryName(desiredPath) ?? string.Empty;
        string stem = Path.GetFileNameWithoutExtension(desiredPath);
        string extension = Path.GetExtension(desiredPath);
        for (int suffix = 2; ; suffix++)
        {
            string candidate = Path.Combine(directory, $"{stem} ({suffix}){extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }
    }
}
