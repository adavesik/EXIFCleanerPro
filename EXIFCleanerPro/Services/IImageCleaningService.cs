namespace EXIFCleanerPro.Services;

internal interface IImageCleaningService
{
    Task<CleaningResult> CleanAsync(
        string inputPath,
        CleaningOptions options,
        CancellationToken cancellationToken);
}
