﻿using Avalonia.Controls;

namespace ElMaDesktop.Classes;

public class NavigationManager
{
    private static ContentControl ContentControl;

    public static void Inizialize(ContentControl _contentControl)
    {
        ContentControl = _contentControl;
    }

    public static void NavigateTo(UserControl userControl)
    {
        ContentControl.Content = userControl;
    }
}