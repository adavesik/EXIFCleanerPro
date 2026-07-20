using EXIFCleanerPro.Services;
using EXIFCleanerPro.ViewModels;
using System.Windows;

namespace EXIFCleanerPro;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ISettingsService settingsService = new JsonSettingsService();
        AppSettings settings = settingsService.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();
        ThemeService themeService = new();
        themeService.Apply(settings.Theme);

        MainViewModel viewModel = new(
            new ImageCleaningService(),
            new MetadataService(),
            new PrivacyReportService(),
            settingsService,
            new WpfFilePickerService(),
            themeService,
            settings);

        MainWindow window = new(viewModel);
        MainWindow = window;
        window.Show();
    }
}
