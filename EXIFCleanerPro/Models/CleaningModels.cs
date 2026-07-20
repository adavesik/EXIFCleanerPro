namespace EXIFCleanerPro;

internal sealed record CleaningOptions(OutputMode OutputMode, string? DestinationFolder);

internal sealed record CleaningResult(
    bool Success,
    string InputPath,
    string? OutputPath,
    string? BackupPath,
    string? ErrorMessage,
    bool Cancelled = false);

internal sealed record OutputModeChoice(OutputMode Mode, string Label, string Description);
