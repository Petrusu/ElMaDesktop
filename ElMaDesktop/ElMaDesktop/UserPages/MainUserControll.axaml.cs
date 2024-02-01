using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ElMaDesktop.UserPages;

public partial class MainUserControll : UserControl
{
    public MainUserControll()
    {
        InitializeComponent();
    }
    public MainUserControll(string jwt)
    {
        InitializeComponent();
    }
}