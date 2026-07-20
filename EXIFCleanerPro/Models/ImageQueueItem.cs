using CommunityToolkit.Mvvm.ComponentModel;

namespace EXIFCleanerPro;

internal partial class ImageQueueItem : ObservableObject
{
    public ImageQueueItem(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
        ParentPath = Path.GetDirectoryName(filePath) ?? string.Empty;
        Extension = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant();
        long bytes = new FileInfo(filePath).Length;
        FileSize = FormatSize(bytes);
    }

    public string FilePath { get; }
    public string FileName { get; }
    public string ParentPath { get; }
    public string Extension { get; }
    public string FileSize { get; }

    [ObservableProperty]
    private bool isSelected = true;

    [ObservableProperty]
    private QueueItemStatus status = QueueItemStatus.Ready;

    public string StatusText => Status switch
    {
        QueueItemStatus.Verified => "Verified",
        QueueItemStatus.VerificationFailed => "Verify warning",
        _ => Status.ToString()
    };

    [ObservableProperty]
    private string metadataSummary = "Scanning…";

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? outputPath;

    [ObservableProperty]
    private bool hasGps;

    [ObservableProperty]
    private bool hasCamera;

    [ObservableProperty]
    private bool hasDate;

    [ObservableProperty]
    private bool hasSoftware;

    [ObservableProperty]
    private PrivacyAssessment assessment = PrivacyAssessment.Empty;

    [ObservableProperty]
    private MetadataComparison? comparison;

    [ObservableProperty]
    private Uri? mapUri;

    public bool HasComparison => Comparison is not null;
    public bool HasMap => MapUri is not null;

    partial void OnComparisonChanged(MetadataComparison? value) => OnPropertyChanged(nameof(HasComparison));
    partial void OnMapUriChanged(Uri? value) => OnPropertyChanged(nameof(HasMap));
    partial void OnStatusChanged(QueueItemStatus value) => OnPropertyChanged(nameof(StatusText));

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        int unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#} {units[unit]}";
    }
}
