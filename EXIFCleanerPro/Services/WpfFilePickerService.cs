using Microsoft.Win32;
using System.Windows;

namespace EXIFCleanerPro.Services;

internal sealed class WpfFilePickerService : IFilePickerService
{
    public IReadOnlyList<string> PickImageFiles()
    {
        OpenFileDialog dialog = new()
        {
            Title = "Add images",
            Multiselect = true,
            CheckFileExists = true,
            Filter = "Supported images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };

        return dialog.ShowDialog(Application.Current.MainWindow) == true
            ? dialog.FileNames
            : [];
    }

    public string? PickFolder(string title, string? initialDirectory = null)
    {
        OpenFolderDialog dialog = new()
        {
            Title = title,
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
        {
            dialog.InitialDirectory = initialDirectory;
        }

        return dialog.ShowDialog(Application.Current.MainWindow) == true
            ? dialog.FolderName
            : null;
    }

    public string? PickSaveFile(string title, string suggestedFileName, string filter, string defaultExtension)
    {
        SaveFileDialog dialog = new()
        {
            Title = title,
            FileName = suggestedFileName,
            Filter = filter,
            DefaultExt = defaultExtension,
            AddExtension = true,
            OverwritePrompt = true
        };

        return dialog.ShowDialog(Application.Current.MainWindow) == true
            ? dialog.FileName
            : null;
    }
}
