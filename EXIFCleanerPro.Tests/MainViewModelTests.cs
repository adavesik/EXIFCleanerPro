using EXIFCleanerPro.Services;
using EXIFCleanerPro.ViewModels;

namespace EXIFCleanerPro.Tests;

public sealed class MainViewModelTests : IDisposable
{
    private readonly string testDirectory = Path.Combine(Path.GetTempPath(), $"ExifCleanerQueue-{Guid.NewGuid():N}");

    public MainViewModelTests() => Directory.CreateDirectory(testDirectory);

    [Fact]
    public async Task DroppedPathsFilterUnsupportedFilesAndIgnoreDuplicates()
    {
        string jpg = CreateFile("one.jpg");
        CreateFile("notes.txt");
        MainViewModel viewModel = CreateViewModel();

        await viewModel.AddDroppedPathsAsync([testDirectory, jpg]);

        Assert.Single(viewModel.Items);
        Assert.Equal(jpg, viewModel.Items[0].FilePath);
        Assert.Contains("duplicate", viewModel.NotificationMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FolderRecursionIsControlledBySetting()
    {
        string child = Path.Combine(testDirectory, "child");
        Directory.CreateDirectory(child);
        CreateFile("root.png");
        File.WriteAllText(Path.Combine(child, "nested.jpg"), "nested");
        MainViewModel viewModel = CreateViewModel();

        await viewModel.AddDroppedPathsAsync([testDirectory]);
        Assert.Single(viewModel.Items);

        viewModel.ClearQueueCommand.Execute(null);
        viewModel.IncludeSubfolders = true;
        await viewModel.AddDroppedPathsAsync([testDirectory]);
        Assert.Equal(2, viewModel.Items.Count);
    }

    [Fact]
    public async Task CleaningReReadsOutputAndMarksItemVerified()
    {
        string jpg = CreateFile("verify.jpg");
        MetadataResult before = CreateMetadataResult(new MetadataEntry("Exif", "Software", "Editor"));
        MetadataResult after = CreateMetadataResult(new MetadataEntry("JPEG", "Image Width", "100 pixels"));
        SequenceMetadataService metadataService = new(before, before, after);
        MainViewModel viewModel = CreateViewModel(metadataService);
        await viewModel.AddDroppedPathsAsync([jpg]);

        await viewModel.CleanCommand.ExecuteAsync(null);

        ImageQueueItem item = Assert.Single(viewModel.Items);
        Assert.Equal(QueueItemStatus.Verified, item.Status);
        Assert.NotNull(item.Comparison);
        Assert.True(item.Comparison.VerificationPassed);
        Assert.Contains("1 verified", viewModel.CompletionSummary, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(3, metadataService.CallCount);
    }

    public void Dispose() => Directory.Delete(testDirectory, true);

    private string CreateFile(string name)
    {
        string path = Path.Combine(testDirectory, name);
        File.WriteAllText(path, name);
        return path;
    }

    private static MainViewModel CreateViewModel(IMetadataService? metadataService = null) => new(
        new FakeCleaningService(),
        metadataService ?? new FakeMetadataService(),
        new FakePrivacyReportService(),
        new MemorySettingsService(),
        new EmptyFilePickerService(),
        new ThemeService(),
        new AppSettings());

    private static MetadataResult CreateMetadataResult(params MetadataEntry[] raw)
    {
        MetadataInterpretation interpretation = MetadataPrivacyAnalyzer.Interpret(raw);
        return new MetadataResult(
            interpretation.Entries,
            false,
            false,
            false,
            raw.Any(entry => entry.Tag.Contains("Software", StringComparison.OrdinalIgnoreCase)),
            interpretation.Assessment);
    }

    private sealed class FakeCleaningService : IImageCleaningService
    {
        public Task<CleaningResult> CleanAsync(string inputPath, CleaningOptions options, CancellationToken cancellationToken) =>
            Task.FromResult(new CleaningResult(true, inputPath, inputPath, null, null));
    }

    private sealed class FakeMetadataService : IMetadataService
    {
        public Task<MetadataResult> ReadAsync(string filePath, CancellationToken cancellationToken) =>
            Task.FromResult(new MetadataResult([], false, false, false, false));
    }

    private sealed class SequenceMetadataService(params MetadataResult[] results) : IMetadataService
    {
        private readonly Queue<MetadataResult> results = new(results);

        public int CallCount { get; private set; }

        public Task<MetadataResult> ReadAsync(string filePath, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(results.Dequeue());
        }
    }

    private sealed class FakePrivacyReportService : IPrivacyReportService
    {
        public Task WriteHtmlAsync(string outputPath, PrivacyReportData report, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class MemorySettingsService : ISettingsService
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new AppSettings());
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class EmptyFilePickerService : IFilePickerService
    {
        public IReadOnlyList<string> PickImageFiles() => [];
        public string? PickFolder(string title, string? initialDirectory = null) => null;
        public string? PickSaveFile(string title, string suggestedFileName, string filter, string defaultExtension) => null;
    }
}
