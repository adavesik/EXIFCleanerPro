namespace EXIFCleanerPro;

internal sealed class AppSettings
{
    public ThemePreference Theme { get; set; } = ThemePreference.System;
    public OutputMode DefaultOutputMode { get; set; } = OutputMode.CleanedCopy;
    public bool IncludeSubfolders { get; set; }
    public bool OpenOutputAfterCompletion { get; set; }
    public string? LastOutputFolder { get; set; }
}

internal enum ThemePreference
{
    System,
    Light,
    Dark
}

internal enum OutputMode
{
    CleanedCopy,
    SelectedFolder,
    ReplaceWithBackup
}

public enum NavigationPage
{
    Cleaner,
    History,
    Settings
}

internal enum QueueItemStatus
{
    Ready,
    Cleaning,
    Verified,
    VerificationFailed,
    Failed,
    Skipped,
    Cancelled
}
