namespace ElMaDesktop.Classes;

using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Controls.Documents;

public class TextFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text && parameter is string label)
        {
            return TextFormatter.FormatText(label, text);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}