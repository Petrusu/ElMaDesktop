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
using System.Threading.Tasks;

namespace ElMaDesktop.Classes;

public partial class AddEditUserControll : UserControl
{
    public AddEditUserControll()
    {
        InitializeComponent();
    }
    public AddEditUserControll(int id)
    {
        InitializeComponent();
    }

    private void Save2Btn_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void Save1Btn_OnClick(object? sender, RoutedEventArgs e)
    {
        BooksCard booksCard = new BooksCard
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
        AddNewBook(booksCard);
    }

    private void ImageSaveBtn_OnClick(object? sender, RoutedEventArgs e)
    {
       
    }
    private async void AddNewBook(BooksCard bookRequest)
    {
        // Пример использования HttpClient:
        using (HttpClient client = new HttpClient())
        {
            // Преобразование объекта в JSON
            string jsonRequest = JsonConvert.SerializeObject(bookRequest);

            // Определение адреса сервера, например:
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
    
}