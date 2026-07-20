namespace EXIFCleanerPro.Services;

internal interface IFilePickerService
{
    IReadOnlyList<string> PickImageFiles();
    string? PickFolder(string title, string? initialDirectory = null);
    string? PickSaveFile(string title, string suggestedFileName, string filter, string defaultExtension);
}
