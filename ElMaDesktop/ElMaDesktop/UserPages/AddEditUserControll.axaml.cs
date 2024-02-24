using System;
using System.Collections.Generic;
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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace ElMaDesktop.Classes;

public partial class AddEditUserControll : UserControl
{
    private BookRequest booksCard;
    byte[] imageBytes;
    private List<int> selectedThemeIds = new List<int>();
    private int selectedThemeId = -1; 

    
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
        SearchThemeBox = this.Find<TextBox>("SearchThemeBox");
        AddThemeTextBox = this.Find<TextBox>("AddThemeTextBox");
        LoadThemes();
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
        ThemesListBox = this.Find<ListBox>("ThemesListBox");
        SearchThemeBox = this.Find<TextBox>("SearchThemeBox");
        AddThemeTextBox = this.Find<TextBox>("AddThemeTextBox");
        BBK = this.Find<TextBox>("BBK");
        LoadThemes();
    }

    private void Save2Btn_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void Save1Btn_OnClick(object? sender, RoutedEventArgs e)
    {
        booksCard = new BookRequest
        {
            Title = Title.Text,
            SeriesName = SeriesName.Text,
            AuthorBook = Author.Text,
            Editor = Editor.Text,
            Publisher = Publisher.Text,
            PlaceOfPublication = PlaceOfPublication.Text,
            YearOfPublication = DateOnly.Parse(YearOfPublication.Text),
            Annotation = Annotation.Text,
            //Image = imageBytes, !warning
            BBK = BBK.Text
        };
        if (selectedThemeIds.Count > 0)
        {
            booksCard.Themes = selectedThemeIds;
        }
        AddNewBook(booksCard);
    }

    private async void ImageSaveBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filters.Add(new FileDialogFilter() { Name = "Images", Extensions = { "jpg", "jpeg", "png" } });
        
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            var selectedFiles = await openFileDialog.ShowAsync(window);
            if (selectedFiles != null && selectedFiles.Length > 0)
            {
                string selectedImagePath = selectedFiles[0];
                try
                {
                    using (var stream = File.OpenRead(selectedImagePath))
                    {
                        var bitmap = new Bitmap(stream);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ImageBook.Source = bitmap;
                        });
                    }
                }
                catch (Exception ex)
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", $"Ошибка:{ex}",
                            ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
            }
        }

        // Преобразование изображения в массив байтов
        using (MemoryStream ms = new MemoryStream())
        {
            var bitmap = (Bitmap)ImageBook.Source; // Получаем изображение из ImageBook
            bitmap.Save(ms);
            imageBytes = ms.ToArray();
        }
    }
    private async void AddNewBook(BookRequest bookRequest)
    {
        using (HttpClient client = new HttpClient())
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize(bookRequest);
        
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
                    .GetMessageBoxStandard("Ошибка", $"Ошибка:{response}",
                        ButtonEnum.Ok);

                var result = await box.ShowAsync();
            }
        }
    }
     private async void LoadThemes()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("http://localhost:5163/api/ForAllUsers/fillthemes");
                    response.EnsureSuccessStatusCode(); // Генерирует исключение в случае ошибки
    
                    var themesJson = await response.Content.ReadAsStringAsync();
                    var themes = JsonConvert.DeserializeObject<List<Themes>>(themesJson);
                    if (SearchThemeBox != null && !string.IsNullOrEmpty(SearchThemeBox.Text))
                    {
                        themes = themes.Where(t => t.Themesname.ToLower().Contains(SearchThemeBox.Text))
                            .ToList();
                    }
                    ThemesListBox.ItemsSource = themes;
                }
                catch (HttpRequestException ex)
                {
                    return;
                }
            }
        }

        private async void AddThemeBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                
                string theme = AddThemeTextBox.Text; 

  
                string json = JsonConvert.SerializeObject(new { theme });

                // Создание HttpClient
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5163/api/ForAdmin/AddTheme"); 

                    // Отправка POST-запроса
                    HttpResponseMessage response = await client.PostAsync("AddTheme", new StringContent(json, Encoding.UTF8, "application/json"));

                    // Проверка успешности запроса
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var box = MessageBoxManager
                            .GetMessageBoxStandard("Готово", $"Тема добавлена",
                                ButtonEnum.Ok);

                        var result = await box.ShowAsync();
                        AddThemeTextBox.Clear();
                        LoadThemes();
                    }
                    else
                    {
                        var box = MessageBoxManager
                            .GetMessageBoxStandard("Ошибка", $"Ошибка:{response}",
                                ButtonEnum.Ok);

                        var result = await box.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Ошибка", $"Ошибка:{ex}",
                        ButtonEnum.Ok);

                var result = await box.ShowAsync();
            }           
        }
        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radioBtn = (sender as RadioButton);
            if (radioBtn.IsChecked == true)
            {
                selectedThemeIds.Add(int.Parse(radioBtn.Tag.ToString()));
            }
        }

        private void SearchThemeBox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            LoadThemes();
        }
}