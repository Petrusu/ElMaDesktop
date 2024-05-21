namespace ElMaDesktop.Classes;

using Avalonia.Controls.Documents;
using Avalonia.Media;

public static class TextFormatter
{
    public static InlineCollection FormatText(string label, string value)
    {
        var inlines = new InlineCollection();
        inlines.Add(new Run(label) { FontWeight = FontWeight.Bold });
        inlines.Add(new Run(" "));
        inlines.Add(new Run(value));
        return inlines;
    }
}
