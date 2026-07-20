namespace EXIFCleanerPro.Services;

internal sealed class ImageCleaningService : IImageCleaningService
{
    public Task<CleaningResult> CleanAsync(
        string inputPath,
        CleaningOptions options,
        CancellationToken cancellationToken) =>
        Task.Run(() => Clean(inputPath, options, cancellationToken), CancellationToken.None);

    private static CleaningResult Clean(
        string inputPath,
        CleaningOptions options,
        CancellationToken cancellationToken)
    {
        string? temporaryPath = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            string targetPath = ResolveTargetPath(inputPath, options);
            string targetDirectory = Path.GetDirectoryName(targetPath)
                ?? throw new InvalidOperationException("The output path has no directory.");
            Directory.CreateDirectory(targetDirectory);
            temporaryPath = Path.Combine(targetDirectory, $".{Guid.NewGuid():N}.tmp");

            ExifHelper.CleanExifData(inputPath, temporaryPath);
            cancellationToken.ThrowIfCancellationRequested();

            if (options.OutputMode == OutputMode.ReplaceWithBackup)
            {
                string backupPath = OutputPathResolver.GetBackupPath(inputPath);
                File.Replace(temporaryPath, inputPath, backupPath, true);
                temporaryPath = null;
                return new CleaningResult(true, inputPath, inputPath, backupPath, null);
            }

            File.Move(temporaryPath, targetPath, false);
            temporaryPath = null;
            return new CleaningResult(true, inputPath, targetPath, null, null);
        }
        catch (OperationCanceledException)
        {
            return new CleaningResult(false, inputPath, null, null, null, true);
        }
        catch (Exception ex)
        {
            return new CleaningResult(false, inputPath, null, null, ex.Message);
        }
        finally
        {
            if (temporaryPath is not null)
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (IOException)
                {
                    // Best-effort cleanup. The operation result already captures the primary failure.
                }
                catch (UnauthorizedAccessException)
                {
                    // Best-effort cleanup. The operation result already captures the primary failure.
                }
            }
        }
    }

    private static string ResolveTargetPath(string inputPath, CleaningOptions options) =>
        options.OutputMode switch
        {
            OutputMode.CleanedCopy => OutputPathResolver.GetCleanedCopyPath(inputPath),
            OutputMode.SelectedFolder when !string.IsNullOrWhiteSpace(options.DestinationFolder) =>
                OutputPathResolver.GetDestinationPath(inputPath, options.DestinationFolder),
            OutputMode.SelectedFolder => throw new InvalidOperationException("Choose an output folder before cleaning."),
            OutputMode.ReplaceWithBackup => inputPath,
            _ => throw new ArgumentOutOfRangeException(nameof(options))
        };
}
