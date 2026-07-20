using System.Text.Json;

namespace EXIFCleanerPro.Services;

internal sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string settingsPath;

    public JsonSettingsService(string? settingsPath = null)
    {
        this.settingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EXIFCleanerPro",
            "settings.json");
    }

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                return new AppSettings();
            }

            await using FileStream stream = File.OpenRead(settingsPath);
            AppSettings? settings = await JsonSerializer.DeserializeAsync<AppSettings>(
                stream,
                SerializerOptions,
                cancellationToken);
            return Validate(settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        string? directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = settingsPath + ".tmp";
        await using (FileStream stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
        }

        File.Move(temporaryPath, settingsPath, true);
    }

    private static AppSettings Validate(AppSettings? settings)
    {
        if (settings is null ||
            !Enum.IsDefined(settings.Theme) ||
            !Enum.IsDefined(settings.DefaultOutputMode))
        {
            return new AppSettings();
        }

        if (!string.IsNullOrWhiteSpace(settings.LastOutputFolder))
        {
            settings.LastOutputFolder = Path.GetFullPath(settings.LastOutputFolder);
        }

        return settings;
    }
}
