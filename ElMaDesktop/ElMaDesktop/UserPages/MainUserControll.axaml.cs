﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ElMaDesktop.Classes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;

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
        SearchTextBox = this.Find<TextBox>("SearchTextBox");
        AddBtn = this.Find<Button>("AddBtn");
        SortComboBox = this.Find<ComboBox>("SortComboBox");
        BooksListBox = this.Find<ListBox>("BooksListBox");
        LoadListBox();
    }

    private async Task LoadListBox()
    {
        // Выполните запрос к API для получения данных книг
        using (var httpClient = new HttpClient())
        {
            try
            {
                if (SortComboBox != null && SortComboBox.SelectedIndex != 1)
                {
                    var response = await httpClient.GetAsync("http://localhost:5163/api/ForAllUsers/BooksOrderBy");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();

                        // Преобразуйте полученные данные в объекты класса BooksCard
                        var booksData = JsonConvert.DeserializeObject<List<BooksCard>>(jsonString);

                        // Привяжите объекты к свойству ItemsSource вашего ListBox
                        BooksListBox.ItemsSource = booksData;
                    }
                    else
                    {
                        /*// Обработка ошибки при запросе к API

                        var box = MessageBoxManager
                            .GetMessageBoxStandard("Ошибка", $"Ошибка при получении данных из API",
                                ButtonEnum.Ok);

                        var result = await box.ShowAsync();*/
                    }
                }
                if (SortComboBox != null && SortComboBox.SelectedIndex != 0)
                {
                    var responsee =
                        await httpClient.GetAsync("http://localhost:5163/api/ForAllUsers/BooksOrderByDescending");
                    if (responsee.IsSuccessStatusCode)
                    {
                        var jsonString = await responsee.Content.ReadAsStringAsync();

                        // Преобразуйте полученные данные в объекты класса BooksCard
                        var booksData = JsonConvert.DeserializeObject<List<BooksCard>>(jsonString);

                        // Привяжите объекты к свойству ItemsSource вашего ListBox
                        BooksListBox.ItemsSource = booksData;
                    }
                    else
                    {
                        // Обработка ошибки при запросе к API

                        /*var box = MessageBoxManager
                            .GetMessageBoxStandard("Ошибка", $"Ошибка при получении данных из API",
                                ButtonEnum.Ok);

                        var result = await box.ShowAsync();*/
                    }
                }
                if ( SearchTextBox != null && !string.IsNullOrEmpty(SearchTextBox.Text)) // Проверка наличия текста для поиска
                {
                    var searchResponse = await httpClient.GetAsync($"http://localhost:5163/api/ForAllUsers/Search?searchTerm={Uri.EscapeDataString(SearchTextBox.Text)}");

                    if (searchResponse.IsSuccessStatusCode)
                    {
                        var searchJsonString = await searchResponse.Content.ReadAsStringAsync();
                        var searchBooksData = JsonConvert.DeserializeObject<List<BooksCard>>(searchJsonString);

                        // Привяжите объекты к свойству ItemsSource вашего ListBox
                        BooksListBox.ItemsSource = searchBooksData;
                    }
                    else
                    {
                        // Обработка ошибки при запросе к API для поиска
                        /*var searchErrorBox = MessageBoxManager
                            .GetMessageBoxStandard("Ошибка", $"Ошибка при выполнении поиска",
                                ButtonEnum.Ok);

                        var searchErrorResult = await searchErrorBox.ShowAsync();*/
                    }
                }
            }
            catch (Exception e)
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", $"Ошибка: {e}",
                        ButtonEnum.Ok);

                var result = await box.ShowAsync();
            }
        }
    }
    
    private void SortComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        LoadListBox();
    }

    private void AddBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void SearchTextBox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        LoadListBox();
    }
}