using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
                var response = await httpClient.GetAsync("http://localhost:5163/api/ForAllUsers/fillbook");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    // Преобразование полученных данных в объекты класса BooksCard
                    var booksData = JsonConvert.DeserializeObject<List<BooksCard>>(jsonString);
                    
                    if (SortComboBox != null && SortComboBox.SelectedIndex != 1)
                    {
                        booksData = booksData.OrderBy(bookOrderBy => bookOrderBy.Title).ToList();
                    }
                    else if (SortComboBox != null && SortComboBox.SelectedIndex != 0)
                    {
                        booksData = booksData.OrderByDescending(bookOrderByDescending => bookOrderByDescending.Title).ToList();
                    }
                    
                    if (SearchTextBox != null && !string.IsNullOrEmpty(SearchTextBox.Text))
                    {
                        string searchText = SearchTextBox.Text.ToLower();
                        booksData = booksData.Where(book => 
                            book.Title.ToLower().Contains(searchText) || 
                            (book.Authors != null && book.Authors.Any(author => author.ToLower().Contains(searchText))) ||
                            (book.Editors != null && book.Editors.Any(editor => editor.ToLower().Contains(searchText)))
                        ).ToList();
                    }

                    else if (SearchTextBox != null && !string.IsNullOrEmpty(SearchTextBox.Text))
                    {
                        booksData = booksData.Where(book => book.Author.ToLower().Contains(SearchTextBox.Text))
                            .ToList();
                    }
                    BooksListBox.ItemsSource = booksData;
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