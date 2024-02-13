using System;
using System.Net.Http;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ElMaDesktop.Classes;

public partial class AddEditUserControll : UserControl
{
    private BooksCard booksCard;
    public AddEditUserControll()
    {
        InitializeComponent();
        Title = this.Find<TextBox>("Title");
        SeriesName = this.Find<TextBox>("SeriesName");
        Author = this.Find<TextBox>("Author");
        Editor = this.Find<TextBox>("Editor");
        Publisher = this.Find<TextBox>("Publisher");
        PlaceOfPublication = this.Find<TextBox>("PlaceOfPublication");
        YearOfPublication = this.Find<TextBox>("YearOfPublication");
        BBK = this.Find<TextBox>("BBK");
        ThemesListBox = this.Find<ListBox>("ThemesListBox");
    }
    public AddEditUserControll(int id)
    {
        InitializeComponent();
        InitializeComponent();
        Title = this.Find<TextBox>("Title");
        SeriesName = this.Find<TextBox>("SeriesName");
        Author = this.Find<TextBox>("Author");
        Editor = this.Find<TextBox>("Editor");
        Publisher = this.Find<TextBox>("Publisher");
        PlaceOfPublication = this.Find<TextBox>("PlaceOfPublication");
        YearOfPublication = this.Find<TextBox>("YearOfPublication");
        BBK = this.Find<TextBox>("BBK");
        ThemesListBox = this.Find<ListBox>("ThemesListBox");
    }

    private void Save2Btn_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void Save1Btn_OnClick(object? sender, RoutedEventArgs e)
    {
        booksCard = new BooksCard
        {
            Title = Title.Text,
            SeriesName = SeriesName.Text,
            Author = Author.Text,
            Editor = Editor.Text,
            Publisher = Publisher.Text,
            PlaceOfPublication = PlaceOfPublication.Text,
            YearOfPublication = YearOfPublication.Text,
            Annotation = Annotation.Text,
        };
    
        // Добавление каждого объекта Themes из ThemesListBox
        foreach (Themes theme in ThemesListBox.Items)
        {
            booksCard.Themes.Add(theme.Theme);
        }
        AddNewBook(booksCard);
    }

    private void ImageSaveBtn_OnClick(object? sender, RoutedEventArgs e)
    {
       
    }
    private async void AddNewBook(BooksCard bookRequest)
    {
        using (HttpClient client = new HttpClient())
        {
            string jsonRequest = JsonConvert.SerializeObject(bookRequest);
        
            string serverUrl = "http://localhost:5163/api/ForAdmin/AddNewBook";

            // Отправка POST-запроса
            HttpResponseMessage response = await client.PostAsync(serverUrl, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            // Обработка ответа от сервера
            if (response.IsSuccessStatusCode)
            {
                // Книга успешно добавлена
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Готово", "Книга успешно добавлена!",
                        ButtonEnum.Ok);

                var result = await box.ShowAsync();
            }
            else
            {
                // Произошла ошибка при добавлении книги
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", $"Ошибка:{response.IsSuccessStatusCode}",
                        ButtonEnum.Ok);

                var result = await box.ShowAsync();
            }
        }
    }

    private void AddThemeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Получение текущего элемента из контекста данных ListBox
        if (sender is Control control)
        {
            if (control.DataContext is Themes currentTheme)
            {
                // Проверка на null и добавление темы
                if (!string.IsNullOrWhiteSpace(currentTheme.Theme))
                {
                    booksCard.Themes.Add(currentTheme.Theme);
                
                    // Очистка текста темы после добавления
                    currentTheme.Theme = string.Empty;
                }
            }
        }
    }
}