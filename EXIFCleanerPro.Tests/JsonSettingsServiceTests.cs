using EXIFCleanerPro.Services;

namespace EXIFCleanerPro.Tests;

public sealed class JsonSettingsServiceTests : IDisposable
{
    private readonly string testDirectory = Path.Combine(Path.GetTempPath(), $"ExifCleanerSettings-{Guid.NewGuid():N}");
    private readonly string settingsPath;

    public JsonSettingsServiceTests()
    {
        Directory.CreateDirectory(testDirectory);
        settingsPath = Path.Combine(testDirectory, "settings.json");
    }

    [Fact]
    public async Task RoundTripsSettings()
    {
        JsonSettingsService service = new(settingsPath);
        AppSettings expected = new()
        {
            Theme = ThemePreference.Dark,
            DefaultOutputMode = OutputMode.SelectedFolder,
            IncludeSubfolders = true,
            OpenOutputAfterCompletion = true,
            LastOutputFolder = testDirectory
        };

        await service.SaveAsync(expected, CancellationToken.None);
        AppSettings actual = await service.LoadAsync(CancellationToken.None);

        Assert.Equal(expected.Theme, actual.Theme);
        Assert.Equal(expected.DefaultOutputMode, actual.DefaultOutputMode);
        Assert.True(actual.IncludeSubfolders);
        Assert.True(actual.OpenOutputAfterCompletion);
        Assert.Equal(testDirectory, actual.LastOutputFolder);
    }

    [Fact]
    public async Task MalformedSettingsFallBackToDefaults()
    {
        await File.WriteAllTextAsync(settingsPath, "{ definitely not json");
        JsonSettingsService service = new(settingsPath);

        AppSettings settings = await service.LoadAsync(CancellationToken.None);

        Assert.Equal(ThemePreference.System, settings.Theme);
        Assert.Equal(OutputMode.CleanedCopy, settings.DefaultOutputMode);
        Assert.False(settings.IncludeSubfolders);
    }

    public void Dispose() => Directory.Delete(testDirectory, true);
}
