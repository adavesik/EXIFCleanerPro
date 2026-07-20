using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EXIFCleanerPro.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageProcessingException = MetadataExtractor.ImageProcessingException;

namespace EXIFCleanerPro.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    private readonly IImageCleaningService cleaningService;
    private readonly IMetadataService metadataService;
    private readonly IPrivacyReportService privacyReportService;
    private readonly ISettingsService settingsService;
    private readonly IFilePickerService filePickerService;
    private readonly ThemeService themeService;
    private readonly HashSet<string> knownPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MetadataEntry> allMetadataEntries = [];
    private CancellationTokenSource? cleaningCancellation;
    private CancellationTokenSource? metadataCancellation;
    private string? lastOutputDirectory;

    public MainViewModel(
        IImageCleaningService cleaningService,
        IMetadataService metadataService,
        IPrivacyReportService privacyReportService,
        ISettingsService settingsService,
        IFilePickerService filePickerService,
        ThemeService themeService,
        AppSettings settings)
    {
        this.cleaningService = cleaningService;
        this.metadataService = metadataService;
        this.privacyReportService = privacyReportService;
        this.settingsService = settingsService;
        this.filePickerService = filePickerService;
        this.themeService = themeService;

        selectedTheme = settings.Theme;
        selectedOutputMode = settings.DefaultOutputMode;
        includeSubfolders = settings.IncludeSubfolders;
        openOutputAfterCompletion = settings.OpenOutputAfterCompletion;
        outputFolder = settings.LastOutputFolder;
        UpdateQueueState();
    }

    public ObservableCollection<ImageQueueItem> Items { get; } = [];
    public ObservableCollection<MetadataEntry> MetadataEntries { get; } = [];

    public IReadOnlyList<OutputModeChoice> OutputModes { get; } =
    [
        new(OutputMode.CleanedCopy, "Cleaned copies", "Create name-cleaned.ext beside each original"),
        new(OutputMode.SelectedFolder, "Selected folder", "Write cleaned images to one destination"),
        new(OutputMode.ReplaceWithBackup, "Replace with backup", "Replace originals and create name-backup.ext")
    ];

    public IReadOnlyList<ThemePreference> Themes { get; } =
    [
        ThemePreference.System,
        ThemePreference.Light,
        ThemePreference.Dark
    ];

    [ObservableProperty]
    private NavigationPage currentPage = NavigationPage.Cleaner;

    [ObservableProperty]
    private ImageQueueItem? selectedItem;

    [ObservableProperty]
    private MetadataEntry? selectedMetadataEntry;

    [ObservableProperty]
    private ImageSource? selectedThumbnail;

    [ObservableProperty]
    private string metadataSearchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool hasItems;

    [ObservableProperty]
    private bool hasSelection;

    [ObservableProperty]
    private int queueCount;

    [ObservableProperty]
    private int selectedCount;

    [ObservableProperty]
    private int progressValue;

    [ObservableProperty]
    private int progressMaximum = 1;

    [ObservableProperty]
    private string statusMessage = "Ready to protect your photos.";

    [ObservableProperty]
    private string? notificationMessage;

    [ObservableProperty]
    private string? completionSummary;

    [ObservableProperty]
    private OutputMode selectedOutputMode;

    [ObservableProperty]
    private string? outputFolder;

    [ObservableProperty]
    private bool includeSubfolders;

    [ObservableProperty]
    private bool openOutputAfterCompletion;

    [ObservableProperty]
    private ThemePreference selectedTheme;

    public string SelectedCountText => $"{SelectedCount} of {QueueCount} selected";
    public string CleanButtonText => SelectedCount == 1 ? "Clean 1 image" : $"Clean {SelectedCount} images";
    public bool HasSelectedItem => SelectedItem is not null;
    public bool IsSelectedFolderMode => SelectedOutputMode == OutputMode.SelectedFolder;
    public bool CanOpenOutput => !string.IsNullOrWhiteSpace(lastOutputDirectory) && Directory.Exists(lastOutputDirectory);

    [RelayCommand]
    private void Navigate(NavigationPage page) => CurrentPage = page;

    [RelayCommand(CanExecute = nameof(CanModifyQueue))]
    private async Task AddFilesAsync()
    {
        IReadOnlyList<string> paths = filePickerService.PickImageFiles();
        await AddPathsAsync(paths);
    }

    [RelayCommand(CanExecute = nameof(CanModifyQueue))]
    private async Task AddFolderAsync()
    {
        string? folder = filePickerService.PickFolder("Add an image folder");
        if (folder is not null)
        {
            await AddPathsAsync([folder]);
        }
    }

    public Task AddDroppedPathsAsync(IEnumerable<string> paths) => AddPathsAsync(paths);

    [RelayCommand(CanExecute = nameof(CanModifyQueue))]
    private void SelectAll()
    {
        foreach (ImageQueueItem item in Items)
        {
            item.IsSelected = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyQueue))]
    private void DeselectAll()
    {
        foreach (ImageQueueItem item in Items)
        {
            item.IsSelected = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveItems))]
    private void RemoveSelected()
    {
        foreach (ImageQueueItem item in Items.Where(item => item.IsSelected).ToList())
        {
            RemoveItem(item);
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyQueue))]
    private void ClearQueue()
    {
        foreach (ImageQueueItem item in Items.ToList())
        {
            RemoveItem(item);
        }

        NotificationMessage = "Queue cleared.";
    }

    [RelayCommand]
    private void BrowseOutputFolder()
    {
        string? folder = filePickerService.PickFolder("Choose an output folder", OutputFolder);
        if (folder is not null)
        {
            OutputFolder = folder;
        }
    }

    [RelayCommand(CanExecute = nameof(CanClean))]
    private async Task CleanAsync()
    {
        List<ImageQueueItem> selectedItems = Items.Where(item => item.IsSelected).ToList();
        if (selectedItems.Count == 0)
        {
            NotificationMessage = "Select at least one image to clean.";
            return;
        }

        if (SelectedOutputMode == OutputMode.SelectedFolder &&
            (string.IsNullOrWhiteSpace(OutputFolder) || !Directory.Exists(OutputFolder)))
        {
            NotificationMessage = "Choose a valid output folder before cleaning.";
            return;
        }

        if (SelectedOutputMode == OutputMode.ReplaceWithBackup)
        {
            MessageBoxResult confirmation = MessageBox.Show(
                $"Replace {selectedItems.Count} original image(s)? A backup will be created beside every original.",
                "Confirm replacement",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel);
            if (confirmation != MessageBoxResult.OK)
            {
                return;
            }
        }

        cleaningCancellation = new CancellationTokenSource();
        CancellationToken cancellationToken = cleaningCancellation.Token;
        IsBusy = true;
        CompletionSummary = null;
        NotificationMessage = null;
        ProgressValue = 0;
        ProgressMaximum = selectedItems.Count;
        StatusMessage = "Cleaning image metadata…";
        int cleaned = 0;
        int verified = 0;
        int verificationWarnings = 0;
        int failed = 0;
        int cancelled = 0;
        HashSet<string> outputDirectories = new(StringComparer.OrdinalIgnoreCase);

        try
        {
            CleaningOptions options = new(SelectedOutputMode, OutputFolder);
            foreach (ImageQueueItem item in selectedItems)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    item.Status = QueueItemStatus.Cancelled;
                    cancelled++;
                    continue;
                }

                item.Status = QueueItemStatus.Cleaning;
                item.ErrorMessage = null;
                item.Comparison = null;
                StatusMessage = $"Cleaning {item.FileName}…";
                MetadataResult before;
                try
                {
                    before = await metadataService.ReadAsync(item.FilePath, cancellationToken);
                    ApplyMetadataSummary(item, before);
                }
                catch (OperationCanceledException)
                {
                    item.Status = QueueItemStatus.Cancelled;
                    cancelled++;
                    ProgressValue++;
                    continue;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ImageProcessingException)
                {
                    item.Status = QueueItemStatus.Failed;
                    item.ErrorMessage = $"Could not capture metadata before cleaning: {ex.Message}";
                    failed++;
                    ProgressValue++;
                    continue;
                }

                CleaningResult result = await cleaningService.CleanAsync(item.FilePath, options, cancellationToken);
                if (result.Cancelled)
                {
                    item.Status = QueueItemStatus.Cancelled;
                    cancelled++;
                }
                else if (result.Success)
                {
                    item.OutputPath = result.OutputPath;
                    cleaned++;
                    string? directory = result.OutputPath is null ? null : Path.GetDirectoryName(result.OutputPath);
                    if (directory is not null)
                    {
                        outputDirectories.Add(directory);
                    }

                    if (result.OutputPath is null)
                    {
                        item.Status = QueueItemStatus.VerificationFailed;
                        item.ErrorMessage = "The cleaner did not return an output path to verify.";
                        verificationWarnings++;
                    }
                    else
                    {
                        try
                        {
                            StatusMessage = $"Verifying {item.FileName}…";
                            MetadataResult after = await metadataService.ReadAsync(result.OutputPath, cancellationToken);
                            item.Comparison = MetadataPrivacyAnalyzer.Compare(before, after);
                            if (item.Comparison.VerificationPassed)
                            {
                                item.Status = QueueItemStatus.Verified;
                                verified++;
                            }
                            else
                            {
                                item.Status = QueueItemStatus.VerificationFailed;
                                item.ErrorMessage = item.Comparison.SensitiveCountSummary;
                                verificationWarnings++;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            item.Status = QueueItemStatus.VerificationFailed;
                            item.ErrorMessage = "Output was written, but verification was cancelled.";
                            verificationWarnings++;
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ImageProcessingException)
                        {
                            item.Status = QueueItemStatus.VerificationFailed;
                            item.ErrorMessage = $"Output was written, but could not be verified: {ex.Message}";
                            verificationWarnings++;
                        }
                    }
                }
                else
                {
                    item.Status = QueueItemStatus.Failed;
                    item.ErrorMessage = result.ErrorMessage;
                    failed++;
                }

                ProgressValue++;
            }
        }
        finally
        {
            IsBusy = false;
            cleaningCancellation.Dispose();
            cleaningCancellation = null;
            lastOutputDirectory = outputDirectories.FirstOrDefault();
            CompletionSummary = $"{verified} verified · {verificationWarnings} warnings · {failed} failed · {cancelled} cancelled";
            StatusMessage = cancelled > 0 ? "Cleaning cancelled." : "Cleaning and verification complete.";
            OnPropertyChanged(nameof(CanOpenOutput));
            OpenOutputCommand.NotifyCanExecuteChanged();
            if (OpenOutputAfterCompletion && cleaned > 0)
            {
                OpenOutput();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void CancelCleaning()
    {
        cleaningCancellation?.Cancel();
        StatusMessage = "Finishing the current image, then stopping…";
    }

    [RelayCommand]
    private void CopySelectedMetadata()
    {
        if (SelectedMetadataEntry is not null)
        {
            Clipboard.SetText(SelectedMetadataEntry.ClipboardText);
            NotificationMessage = "Metadata tag copied.";
        }
    }

    [RelayCommand]
    private void CopyAllMetadata()
    {
        if (allMetadataEntries.Count > 0)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, allMetadataEntries.Select(entry => entry.ClipboardText)));
            NotificationMessage = "All metadata copied.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenMap))]
    private void OpenMap()
    {
        if (SelectedItem?.MapUri is not Uri mapUri)
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = mapUri.AbsoluteUri,
            UseShellExecute = true
        });
    }

    [RelayCommand(CanExecute = nameof(CanExportPrivacyReport))]
    private async Task ExportPrivacyReportAsync()
    {
        if (SelectedItem is not ImageQueueItem item)
        {
            return;
        }

        string suggestedName = $"{Path.GetFileNameWithoutExtension(item.FileName)}-privacy-report.html";
        string? outputPath = filePickerService.PickSaveFile(
            "Export privacy report",
            suggestedName,
            "HTML privacy report (*.html)|*.html",
            ".html");
        if (outputPath is null)
        {
            return;
        }

        try
        {
            PrivacyReportData report = new(
                item.FileName,
                item.FilePath,
                item.Assessment,
                item.Comparison,
                DateTimeOffset.Now);
            await privacyReportService.WriteHtmlAsync(outputPath, report, CancellationToken.None);
            NotificationMessage = $"Privacy report exported to {outputPath}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            NotificationMessage = $"Privacy report could not be exported: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenOutput))]
    private void OpenOutput()
    {
        if (!CanOpenOutput)
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{lastOutputDirectory}\"",
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void ClearNotification() => NotificationMessage = null;

    private bool CanModifyQueue() => !IsBusy;
    private bool CanRemoveItems() => !IsBusy && HasSelection;
    private bool CanClean() => !IsBusy && HasSelection;
    private bool CanOpenMap() => SelectedItem?.HasMap == true;
    private bool CanExportPrivacyReport() => SelectedItem is not null;

    private async Task AddPathsAsync(IEnumerable<string> paths)
    {
        if (IsBusy)
        {
            return;
        }

        NotificationMessage = null;
        DiscoveryResult discovery = await Task.Run(() => DiscoverFiles(paths, IncludeSubfolders));
        int duplicates = discovery.Duplicates;
        List<ImageQueueItem> addedItems = [];

        foreach (string filePath in discovery.Files)
        {
            if (!knownPaths.Add(filePath))
            {
                duplicates++;
                continue;
            }

            ImageQueueItem item = new(filePath);
            item.PropertyChanged += OnQueueItemPropertyChanged;
            Items.Add(item);
            addedItems.Add(item);
        }

        UpdateQueueState();
        await Task.WhenAll(addedItems.Select(PopulateMetadataSummaryAsync));

        List<string> notes = [];
        notes.Add($"Added {addedItems.Count} image{(addedItems.Count == 1 ? string.Empty : "s")}.");
        if (duplicates > 0)
        {
            notes.Add($"{duplicates} duplicate{(duplicates == 1 ? string.Empty : "s")} ignored.");
        }

        if (discovery.Rejected > 0)
        {
            notes.Add($"{discovery.Rejected} unsupported or inaccessible item{(discovery.Rejected == 1 ? string.Empty : "s")} skipped.");
        }

        NotificationMessage = string.Join(" ", notes);
    }

    private static DiscoveryResult DiscoverFiles(IEnumerable<string> inputPaths, bool includeSubfolders)
    {
        HashSet<string> files = new(StringComparer.OrdinalIgnoreCase);
        int rejected = 0;
        int duplicates = 0;
        foreach (string inputPath in inputPaths)
        {
            try
            {
                string fullPath = Path.GetFullPath(inputPath);
                if (File.Exists(fullPath))
                {
                    if (SupportedExtensions.Contains(Path.GetExtension(fullPath)))
                    {
                        if (!files.Add(fullPath))
                        {
                            duplicates++;
                        }
                    }
                    else
                    {
                        rejected++;
                    }

                    continue;
                }

                if (Directory.Exists(fullPath))
                {
                    EnumerationOptions options = new()
                    {
                        RecurseSubdirectories = includeSubfolders,
                        IgnoreInaccessible = true,
                        ReturnSpecialDirectories = false
                    };
                    foreach (string file in Directory.EnumerateFiles(fullPath, "*", options))
                    {
                        if (SupportedExtensions.Contains(Path.GetExtension(file)))
                        {
                            if (!files.Add(Path.GetFullPath(file)))
                            {
                                duplicates++;
                            }
                        }
                    }

                    continue;
                }

                rejected++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                rejected++;
            }
        }

        return new DiscoveryResult(files.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList(), rejected, duplicates);
    }

    private async Task PopulateMetadataSummaryAsync(ImageQueueItem item)
    {
        try
        {
            MetadataResult metadata = await metadataService.ReadAsync(item.FilePath, CancellationToken.None);
            ApplyMetadataSummary(item, metadata);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ImageProcessingException)
        {
            item.MetadataSummary = "Metadata unavailable";
        }
    }

    private void ApplyMetadataSummary(ImageQueueItem item, MetadataResult metadata)
    {
        item.MetadataSummary = metadata.Entries.Count == 1 ? "1 tag" : $"{metadata.Entries.Count} tags";
        item.HasGps = metadata.HasGps;
        item.HasCamera = metadata.HasCamera;
        item.HasDate = metadata.HasDate;
        item.HasSoftware = metadata.HasSoftware;
        item.Assessment = metadata.Assessment;
        item.MapUri = metadata.MapUri;
        if (ReferenceEquals(SelectedItem, item))
        {
            OpenMapCommand.NotifyCanExecuteChanged();
        }
    }

    private void RemoveItem(ImageQueueItem item)
    {
        item.PropertyChanged -= OnQueueItemPropertyChanged;
        knownPaths.Remove(item.FilePath);
        Items.Remove(item);
        if (ReferenceEquals(SelectedItem, item))
        {
            SelectedItem = Items.FirstOrDefault();
        }

        UpdateQueueState();
    }

    private void OnQueueItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageQueueItem.IsSelected))
        {
            UpdateQueueState();
        }
    }

    private void UpdateQueueState()
    {
        QueueCount = Items.Count;
        SelectedCount = Items.Count(item => item.IsSelected);
        HasItems = QueueCount > 0;
        HasSelection = SelectedCount > 0;
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(CleanButtonText));
        AddFilesCommand.NotifyCanExecuteChanged();
        AddFolderCommand.NotifyCanExecuteChanged();
        SelectAllCommand.NotifyCanExecuteChanged();
        DeselectAllCommand.NotifyCanExecuteChanged();
        RemoveSelectedCommand.NotifyCanExecuteChanged();
        ClearQueueCommand.NotifyCanExecuteChanged();
        CleanCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedItemChanged(ImageQueueItem? value)
    {
        OnPropertyChanged(nameof(HasSelectedItem));
        metadataCancellation?.Cancel();
        metadataCancellation?.Dispose();
        metadataCancellation = null;
        allMetadataEntries.Clear();
        MetadataEntries.Clear();
        SelectedThumbnail = null;
        OpenMapCommand.NotifyCanExecuteChanged();
        ExportPrivacyReportCommand.NotifyCanExecuteChanged();

        if (value is not null)
        {
            metadataCancellation = new CancellationTokenSource();
            _ = LoadInspectorAsync(value, metadataCancellation.Token);
        }
    }

    partial void OnMetadataSearchTextChanged(string value) => ApplyMetadataFilter();

    partial void OnIsBusyChanged(bool value)
    {
        UpdateQueueState();
        CancelCleaningCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedOutputModeChanged(OutputMode value)
    {
        OnPropertyChanged(nameof(IsSelectedFolderMode));
        _ = SaveSettingsAsync();
    }

    partial void OnOutputFolderChanged(string? value) => _ = SaveSettingsAsync();
    partial void OnIncludeSubfoldersChanged(bool value) => _ = SaveSettingsAsync();
    partial void OnOpenOutputAfterCompletionChanged(bool value) => _ = SaveSettingsAsync();

    partial void OnSelectedThemeChanged(ThemePreference value)
    {
        themeService.Apply(value);
        _ = SaveSettingsAsync();
    }

    private async Task LoadInspectorAsync(ImageQueueItem item, CancellationToken cancellationToken)
    {
        try
        {
            Task<MetadataResult> metadataTask = metadataService.ReadAsync(item.FilePath, cancellationToken);
            Task<ImageSource?> thumbnailTask = LoadThumbnailAsync(item.FilePath, cancellationToken);
            MetadataResult metadata = await metadataTask;
            SelectedThumbnail = await thumbnailTask;
            cancellationToken.ThrowIfCancellationRequested();

            allMetadataEntries.Clear();
            allMetadataEntries.AddRange(metadata.Entries);
            ApplyMetadataSummary(item, metadata);
            ApplyMetadataFilter();
        }
        catch (OperationCanceledException)
        {
            // Selection changed before the inspector finished loading.
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ImageProcessingException)
        {
            NotificationMessage = $"Could not read metadata for {item.FileName}: {ex.Message}";
        }
    }

    private static Task<ImageSource?> LoadThumbnailAsync(string filePath, CancellationToken cancellationToken) =>
        Task.Run<ImageSource?>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 480;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }, cancellationToken);

    private void ApplyMetadataFilter()
    {
        IEnumerable<MetadataEntry> entries = allMetadataEntries;
        if (!string.IsNullOrWhiteSpace(MetadataSearchText))
        {
            string search = MetadataSearchText.Trim();
            entries = entries.Where(entry => entry.MatchesSearch(search));
        }

        MetadataEntries.Clear();
        foreach (MetadataEntry entry in entries)
        {
            MetadataEntries.Add(entry);
        }
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            AppSettings settings = new()
            {
                Theme = SelectedTheme,
                DefaultOutputMode = SelectedOutputMode,
                IncludeSubfolders = IncludeSubfolders,
                OpenOutputAfterCompletion = OpenOutputAfterCompletion,
                LastOutputFolder = OutputFolder
            };
            await settingsService.SaveAsync(settings, CancellationToken.None);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            NotificationMessage = $"Settings could not be saved: {ex.Message}";
        }
    }

    private sealed record DiscoveryResult(IReadOnlyList<string> Files, int Rejected, int Duplicates);
}
