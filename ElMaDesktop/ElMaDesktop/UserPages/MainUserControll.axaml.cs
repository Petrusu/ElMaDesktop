using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
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

namespace ElMaDesktop.UserPages
{
    public partial class MainUserControll : UserControl, INotifyPropertyChanged
    {
        private int _currentPage = 1;
        private int _totalPages;
        private const int PageSize = 10; // Количество элементов на странице
        private List<BooksCard> _allBooks = new List<BooksCard>();
        private bool isErrorMessageShown = false;
        private bool _isLoading = false;

        public string CurrentPageDisplay => $"Страница {_currentPage} из {_totalPages}";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainUserControll()
        {
            InitializeComponent();
            DataContext = this;
        }

        public MainUserControll(string jwt) : this()
        {
            SearchTextBox = this.Find<TextBox>("SearchTextBox");
            AddBtn = this.Find<Button>("AddBtn");
            BooksListBox = this.Find<ListBox>("BooksListBox");
            LoadingProgressBar = this.FindControl<ProgressBar>("LoadingProgressBar");
            LoadListBox();
        }

        private async Task LoadListBox()
        {
            if (_isLoading) return;
            _isLoading = true;

            LoadingProgressBar.IsVisible = true;
            BooksListBox.IsVisible = false;
            PaginationPanel.IsVisible = false;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync("http://194.146.242.26:7777/api/ForAllUsers/fillbook");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var responseData = JArray.Parse(jsonString); // Используем JArray для разбора массива JSON

                        _allBooks = JsonConvert.DeserializeObject<List<BooksCard>>(responseData.ToString());

                        _totalPages = (int)Math.Ceiling((double)_allBooks.Count / PageSize);
                        OnPropertyChanged(nameof(CurrentPageDisplay));
                        ApplySearchAndDisplayPage();
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
                    var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Ошибка: Не удалось загрузить список.", ButtonEnum.Ok);
                    var result = await box.ShowAsync();
                    isErrorMessageShown = true;
                    Console.WriteLine(e);
                }
            }

            _isLoading = false;
            LoadingProgressBar.IsVisible = false;
            BooksListBox.IsVisible = true;
            PaginationPanel.IsVisible = true;
        }


        private void ApplySearchAndDisplayPage()
        {
            List<BooksCard> filteredBooks = _allBooks;

            if (SearchTextBox != null && !string.IsNullOrEmpty(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                filteredBooks = filteredBooks.Where(book =>
                    (book.Title.ToLower().Contains(searchText)) ||
                    (book.Authors != null && book.Authors.Any(author => author != null && author.ToLower().Contains(searchText))) ||
                    (book.Editors != null && book.Editors.Any(editor => editor != null && editor.ToLower().Contains(searchText)))
                ).ToList();
            }

            _totalPages = (int)Math.Ceiling((double)filteredBooks.Count / PageSize);
            OnPropertyChanged(nameof(CurrentPageDisplay));

            var booksToDisplay = filteredBooks
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .Select(book =>
                {
                    if (book.Image != null)
                    {
                        using (MemoryStream stream = new MemoryStream(book.Image))
                        {
                            book.ImageBook = new Bitmap(stream);
                        }
                    }
                    else
                    {
                        string file = Path.Combine(Directory.GetCurrentDirectory(), "Images").Replace("bin\\Debug\\net7.0", "");
                        try
                        {
                            Bitmap bitmap = new Bitmap(Path.Combine(file, "picture.png"));
                            book.ImageBook = bitmap;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            book.ImageBook = new Bitmap("picture.png");
                        }
                    }

                    book.Authorsname = book.Authors != null ? string.Join(", ", book.Authors) : string.Empty;
                    book.Editorname = book.Editors != null ? string.Join(", ", book.Editors) : string.Empty;

                    return book;
                })
                .ToList();

            BooksListBox.ItemsSource = booksToDisplay;
        }

        private void AddBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            AddEditUserControll addUserControll = new AddEditUserControll();
            NavigationManager.NavigateTo(addUserControll);
        }

        private void SearchTextBox_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            _currentPage = 1;
            ApplySearchAndDisplayPage();
        }

        private async void DeliteBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                var Id = (sender as Button).Tag.ToString();
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"http://194.146.242.26:7777/api/ForAdmin/DeleteBook?bookId={Id}";
                    HttpResponseMessage response = await client.PostAsync(apiUrl, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Готово", "Книга успешно удалена!", ButtonEnum.Ok);
                        var result = await box.ShowAsync();
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Произошла ошибка при удалении книги.", ButtonEnum.Ok);
                        var result = await box.ShowAsync();
                        isErrorMessageShown = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Ошибка: Сервер не отвечает.", ButtonEnum.Ok);
                var result = await box.ShowAsync();
                isErrorMessageShown = true;
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
                    var response = await httpClient.GetAsync($"http://194.146.242.26:7777/api/ForAllUsers/getinformationaboutbook?bookId={selectedBook.BookId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        book = System.Text.Json.JsonSerializer.Deserialize<BookRequest>(jsonString);
                    }
                }
                catch (Exception ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Ошибка", "Ошибка: Сервер не отвечает.", ButtonEnum.Ok);
                    var result = await box.ShowAsync();
                    isErrorMessageShown = true;
                }
            }

            var editUserControll = new AddEditUserControll(book);
            NavigationManager.NavigateTo(editUserControll);
        }

        private void PreviousPageBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                OnPropertyChanged(nameof(CurrentPageDisplay));
                ApplySearchAndDisplayPage();
            }
        }

        private void NextPageBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                OnPropertyChanged(nameof(CurrentPageDisplay));
                ApplySearchAndDisplayPage();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
