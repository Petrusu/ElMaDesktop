using Avalonia.Controls;
using ElMaDesktop.Classes;
using ElMaDesktop.UserPages;

namespace ElMaDesktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ContentControl = this.FindControl<ContentControl>("ContentControl");
        NavigationManager.Inizialize(ContentControl);
        ShowUserControll();
    }

    private void ShowUserControll()
    {
        ContentControl.Content = new AutorizationUserControl();
    }
}