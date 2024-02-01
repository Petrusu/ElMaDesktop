using System;
using System.Net.Http;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ElMaDesktop.Classes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;

namespace ElMaDesktop.UserPages;

public partial class AutorizationUserControl : UserControl
{
    public AutorizationUserControl()
    {
        InitializeComponent();
        LoginTextBox = this.Find<TextBox>("LoginTextBox");
        PasswordTextBox = this.Find<TextBox>("PasswordTextBox");
    }

    private async void AuthDtn_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            //создаем экземпляр HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Отправляем POST-запрос к локальному API
                HttpResponseMessage responseMessage =
                    await client.PostAsync(
                        $"http://localhost:5163/api/ForAllUsers/login?login={LoginTextBox.Text}&password={PasswordTextBox.Text}",
                        null);

                // Проверяем успешность запроса
                if (responseMessage.IsSuccessStatusCode)
                {
                    // Читаем содержимое ответа
                    string responseBody = await responseMessage.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    string
                        jwt = responseObject.loginResponse
                            .token; // Извлекаем токен из поля "token" внутри "loginResponse"

                    MainUserControll mainPage = new MainUserControll(jwt);
                    NavigationManager.NavigateTo(mainPage);
                }
                else
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Ошибка", $"Ошибка: {responseMessage.StatusCode}, проверьте логин и пароль",
                            ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
            }
        }
        catch (Exception exception)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Ошибка", $"Ошибка:{exception}",
                    ButtonEnum.Ok);

            var result = await box.ShowAsync();
        }
        
            
    }
}