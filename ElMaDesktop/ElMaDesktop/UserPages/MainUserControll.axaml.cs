using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ElMaDesktop.Classes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElMaDesktop.UserPages;

public partial class MainUserControll : UserControl
{
    private int _currentPage = 1;
    private int _totalPages;

    public string CurrentPageDisplay => $"Страница {_currentPage} из {_totalPages}";

    public MainUserControll()
    {
        InitializeComponent();
    }

    public MainUserControll(string jwt)
    {
        InitializeComponent();
        SearchTextBox = this.Find<TextBox>("SearchTextBox");
        AddBtn = this.Find<Button>("AddBtn");
        BooksListBox = this.Find<ListBox>("BooksListBox");
        LoadingProgressBar = this.Find<ProgressBar>("LoadingProgressBar");
        LoadListBox();
    }

   private async Task LoadListBox(int page = 1)
{
    LoadingProgressBar.Value = 0;

    using (var httpClient = new HttpClient())
    {
        try
        {
            var response = await httpClient.GetAsync($"http://localhost:5163/api/ForAllUsers/fillbookOnPageDesktop?page={page}&size=20");
            if (LoadingProgressBar != null)
            {
                LoadingProgressBar.Value = 10;
            }
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseData = JObject.Parse(jsonString);

                var booksData = JsonConvert.DeserializeObject<List<BooksCard>>(responseData["Books"].ToString());
                _totalPages = (int)responseData["TotalPages"];

                if (booksData != null && SearchTextBox != null && !string.IsNullOrEmpty(SearchTextBox.Text))
                {
                    string searchText = SearchTextBox.Text.ToLower();
                    booksData = booksData.Where(book => 
                        book.Title.ToLower().Contains(searchText) || 
                        (book.Authors != null && book.Authors.Any(author => author.ToLower().Contains(searchText))) ||
                        (book.Editors != null && book.Editors.Any(editor => editor.ToLower().Contains(searchText)))
                    ).ToList();
                }

                List<BooksCard> booksCards = new List<BooksCard>();
                int totalBooks = booksData.Count;

                if (LoadingProgressBar != null)
                {
                    LoadingProgressBar.Value = 50;
                }

                foreach (var book in booksData)
                {
                    BooksCard booksCard = new BooksCard
                    {
                        BookId = book.BookId,
                        BBK = book.BBK,
                        Title = book.Title,
                        SeriesName = book.SeriesName,
                        Publisher = book.Publisher,
                        PlaceOfPublication = book.PlaceOfPublication,
                        YearOfPublication = book.YearOfPublication
                    };

                    if (book.Image != null)
                    {
                        using (MemoryStream stream = new MemoryStream(book.Image))
                        {
                            booksCard.ImageBook = new Bitmap(stream);
                        }
                    }
                    else
                    {
                        string file = Path.Combine(Directory.GetCurrentDirectory(), "Images").Replace("bin\\Debug\\net7.0", "");
                        try
                        {
                            Bitmap bitmap = new Bitmap(Path.Combine(file, "picture.png"));
                            booksCard.ImageBook = bitmap;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            booksCard.ImageBook = new Bitmap("picture.png");
                        }
                    }

                    if (book.Authors != null && book.Authors.Count != 0)
                    {
                        booksCard.Authorsname = string.Join(", ", book.Authors);
                    }
                    if (book.Editors != null && book.Editors.Count != 0)
                    {
                        booksCard.Editorname = string.Join(", ", book.Editors);
                    }

                    booksCards.Add(booksCard);
                }

                if (LoadingProgressBar != null)
                {
                    LoadingProgressBar.Value = 100;
                }

                BooksListBox.ItemsSource = booksCards;
            }
            else
            {
                Console.WriteLine("Response status code: " + response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response error content: " + errorContent);
            }
        }
        catch (Exception e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Ошибка: {e.Message}", ButtonEnum.Ok);
            var result = await box.ShowAsync();
            Console.WriteLine(e);
        }
    }
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

    private async void DeliteBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var Id = (sender as Button).Tag.ToString();
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"http://localhost:5163/api/ForAdmin/DeleteBook?bookId={Id}";
                HttpResponseMessage response = await client.PostAsync(apiUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Готово", "Книга успешно удалена!", ButtonEnum.Ok);
                    var result = await box.ShowAsync();
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Произошла ошибка при удалении книги. Код ошибки:{response.StatusCode}", ButtonEnum.Ok);
                    var result = await box.ShowAsync();
                }
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Ошибка: {ex.Message}", ButtonEnum.Ok);
            var result = await box.ShowAsync();
        }

        LoadListBox();
    }

    private async void BooksListBox_OnSelectionChanged(object? sender, TappedEventArgs tappedEventArgs)
    {
        var selectedBook = ((sender as ListBox).SelectedItem as BooksCard);
        if (selectedBook == null)
        {
            return;
        }
        BookRequest book = new BookRequest();
        using (var httpClient = new HttpClient())
        {
            try
            {
                var response = await httpClient.GetAsync($"http://localhost:5163/api/ForAllUsers/getinformationaboutbook?bookId={selectedBook.BookId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    book = System.Text.Json.JsonSerializer.Deserialize<BookRequest>(jsonString);
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", $"Ошибка: {ex.Message}", ButtonEnum.Ok);
                var result = await box.ShowAsync();
            }
        }

        BookRequest br = new BookRequest
        {
            Title = book.Title,
            Annotation = book.Annotation,
            AuthorBook = book.AuthorBook,
            SeriesName = book.SeriesName,
            Publisher = book.Publisher,
            PlaceOfPublication = book.PlaceOfPublication,
            YearOfPublication = book.YearOfPublication,
            BBK = book.BBK,
            Editor = book.Editor,
            Id = book.Id,
            Image = book.Image,
            Themes = book.Themes,
            ImageName = book.ImageName
        };

        AddEditUserControll editUserControll = new AddEditUserControll(br);
        NavigationManager.NavigateTo(editUserControll);
    }

    private void PreviousPageBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            LoadListBox(_currentPage);
        }
    }

    private void NextPageBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            LoadListBox(_currentPage);
        }
    }
}
