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
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ElMaDesktop.UserPages;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ElMaDesktop.Classes;

public partial class AddEditUserControll : UserControl
{
    BookRequest booksCard;
    private byte[] imageBytes;
    private string imageName;
    private List<int> selectedThemeIds = new List<int>();
    private int selectedThemeId = -1;
    private bool editMode = false;


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

    public AddEditUserControll(BookRequest bookRequest)
    {
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
        editMode = true;
        LoadThemes();
        booksCard = bookRequest; 
        LoadEditBook(bookRequest);
    }

    private async void LoadEditBook(BookRequest book)
    {
        Title.Text = book.Title;
        Author.Text = "";
        Editor.Text = "";
        foreach (var autor in book.AuthorBook)
        {
            Author.Text += Author.Text.Length == 0 ? autor : ", " + autor;
        }
        foreach (var editor in book.Editor)
        {
            Editor.Text += Editor.Text.Length == 0 ? editor : ", " + editor;
        }
        Annotation.Text = book.Annotation;
        Publisher.Text = book.Publisher;
        SeriesName.Text = book.SeriesName;
        BBK.Text = book.BBK;
        PlaceOfPublication.Text = book.PlaceOfPublication;
        YearOfPublication.Text = book.YearOfPublication.ToString();
        imageName = book.ImageName;
        imageBytes = book.Image;
        var file = Path.Combine(Directory.GetCurrentDirectory(), "Images").Replace("\\bin\\Debug\\net7.0", "");
        try
        {
            ImageBook.Source = new Bitmap(new MemoryStream(booksCard.Image));
        }
        catch (Exception e)
        {
            Bitmap bitmap = new Bitmap(Path.Combine(file, "picture.png"));
            ImageBook.Source = bitmap;
        }
    }

    private void Save1Btn_OnClick(object? sender, RoutedEventArgs e) //добавление
    {
        var bookCard = new BookRequest 
        {
            Title = Title.Text,
            SeriesName = SeriesName.Text == null ? "" : SeriesName.Text,
            AuthorBook = Author.Text== null ? new string []{} : Author.Text.Split(", "),
            Editor = Editor.Text == null ? new string []{} : Editor.Text.Split(", "),
            Publisher = Publisher.Text,
            PlaceOfPublication = PlaceOfPublication.Text,
            YearOfPublication = int.Parse(YearOfPublication.Text),
            Annotation = Annotation.Text == null ? " " : Annotation.Text,
            Image = imageBytes == null ? new byte[]{} : imageBytes,
            ImageName = imageName != "" && imageName != null ? imageName : "picture.png", 
            BBK = BBK.Text
        };
        if (editMode)
        {
            bookCard.Id = booksCard.Id;
        }
        if (selectedThemeIds.Count > 0)
        {
            List<int> newThemesList = new List<int>();
            foreach (var theme in ThemesListBox.Items)
            {
                ThemesListItem themesListItem = theme as ThemesListItem;
                if (themesListItem.IsActive)
                {
                    newThemesList.Add(themesListItem.ThemesId);   
                }
            }
            bookCard.Themes = newThemesList; 
        }
        
        AddNewBook(bookCard);
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
                        await Dispatcher.UIThread.InvokeAsync(() => { ImageBook.Source = bitmap; });
                        imageName = DateTime.Now.Microsecond.ToString() + DateTime.Now.Millisecond.ToString() + "." + selectedFiles[0].Split(".").Last();
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

            string serverUrl = "";

            if (editMode)
            {
                serverUrl = "http://194.146.242.26:7777/api/ForAdmin/EditBook";
            }
            else
            {
                serverUrl = "http://194.146.242.26:7777/api/ForAdmin/AddNewBook";
            }

            // Отправка POST-запроса
            HttpResponseMessage response = await client.PostAsync(serverUrl,
                new StringContent(jsonRequest, Encoding.UTF8, "application/json"));

            // Обработка ответа от сервера
            if (response.IsSuccessStatusCode)
            {
                if (editMode)
                {
                    // Книга успешно отредактированна
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Готово", "Книга успешно отредактированна!",
                            ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
                else
                {
                    // Книга успешно добавлена
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Готово", "Книга успешно добавлена!",
                            ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
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
                var response = await client.GetAsync("http://194.146.242.26:7777/api/ForAllUsers/fillthemes");
                response.EnsureSuccessStatusCode(); // Генерирует исключение в случае ошибки

                var themesJson = await response.Content.ReadAsStringAsync();
                var themes = JsonSerializer.Deserialize<List<Themes>>(themesJson);
                if (SearchThemeBox != null && !string.IsNullOrEmpty(SearchThemeBox.Text))
                {
                    themes = themes.Where(t => t.Themesname.ToLower().Contains(SearchThemeBox.Text))
                        .ToList();
                }
                var tempList = new List<ThemesListItem>();
                foreach (var theme in themes)
                {
                    tempList.Add(new ThemesListItem()
                    {
                        Themesname = theme.Themesname,
                        ThemesId = theme.ThemesId,
                        IsActive = false
                    });
                }
                ThemesListBox.ItemsSource = tempList;
            }
            catch (HttpRequestException ex)
            {
                return;
            }
        }

        if (editMode && booksCard.Themes.Count != 0)
        {
            foreach (var theme in booksCard.Themes)
            {
                (ThemesListBox.Items.Where(i => (i as ThemesListItem).ThemesId == theme).First() as ThemesListItem).IsActive = true;
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
                client.BaseAddress = new Uri("http://194.146.242.26:7777/api/ForAdmin/AddTheme");

                // Отправка POST-запроса
                HttpResponseMessage response = await client.PostAsync("AddTheme",
                    new StringContent(json, Encoding.UTF8, "application/json"));

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
        var chkBox = (sender as CheckBox);
        if (chkBox.IsChecked == true)
        {
            selectedThemeIds.Add(int.Parse(chkBox.Tag.ToString()));
        }
    }

    private void SearchThemeBox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        LoadThemes();
    }

    private void BackBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        MainUserControll mainUserControll = new MainUserControll();
        NavigationManager.NavigateTo(mainUserControll);
    }
}