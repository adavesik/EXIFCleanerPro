using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EXIFCleanerPro.Converters;

public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool inverted = value is bool flag && !flag;
        return targetType == typeof(Visibility)
            ? inverted ? Visibility.Visible : Visibility.Collapsed
            : inverted;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            bool flag => !flag,
            Visibility visibility => visibility != Visibility.Visible,
            _ => false
        };
}
