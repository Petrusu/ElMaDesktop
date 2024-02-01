using System;
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
        //Запрос к API для получения данных книг
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
                        // Преобразование полученных данных в объекты класса BooksCard
                        var booksData = JsonConvert.DeserializeObject<List<BooksCard>>(jsonString);
                        
                        BooksListBox.ItemsSource = booksData;
                    }
                }
                if (SortComboBox != null && SortComboBox.SelectedIndex != 0)
                {
                    var responsee =
                        await httpClient.GetAsync("http://localhost:5163/api/ForAllUsers/BooksOrderByDescending");
                    if (responsee.IsSuccessStatusCode)
                    {
                        var jsonString = await responsee.Content.ReadAsStringAsync();
                        // Преобразование полученных данных в объекты класса BooksCard
                        var booksData = JsonConvert.DeserializeObject<List<BooksCard>>(jsonString);
                        
                        BooksListBox.ItemsSource = booksData;
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
        AddEditUserControll addUserControll = new AddEditUserControll();
        NavigationManager.NavigateTo(addUserControll);
    }

    private void SearchTextBox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        LoadListBox();
    }

    private void EditBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        var Id = (sender as Button).Tag.ToString();
        AddEditUserControll editUserControll = new AddEditUserControll(Convert.ToUInt16(Id));
        NavigationManager.NavigateTo(editUserControll);
    }

    private async void DeliteBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Получение Id книги из Tag кнопки
            var Id = (sender as Button).Tag.ToString();

            // Создание HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Задание URL для удаления книги
                string apiUrl = "http://localhost:5163/api/ForAdmin/DeleteBook?bookId=" + Id;

                // Отправка DELETE-запроса на сервер
                HttpResponseMessage response = await client.DeleteAsync(apiUrl);

                // Проверка успешности операции
                if (response.IsSuccessStatusCode)
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Готово", "Книга успешно удалена!",
                            ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
                else
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", $"Произошла ошибка при удалении книги. Код ошибки:{response.StatusCode}",
                            ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Ошибка", $"Ошибка: {e}",
                    ButtonEnum.Ok);

            var result = await box.ShowAsync();
        }
    }
}