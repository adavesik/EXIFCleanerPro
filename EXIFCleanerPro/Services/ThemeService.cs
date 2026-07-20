using System.Windows;

namespace EXIFCleanerPro.Services;

internal sealed class ThemeService
{
    public void Apply(ThemePreference preference)
    {
#pragma warning disable WPF0001
        Application.Current.ThemeMode = preference switch
        {
            ThemePreference.Light => ThemeMode.Light,
            ThemePreference.Dark => ThemeMode.Dark,
            _ => ThemeMode.System
        };
#pragma warning restore WPF0001
    }
}
