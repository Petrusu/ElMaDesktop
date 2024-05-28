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
        LoginTextBox.Text = "ElMaAdmin";
        PasswordTextBox.Text = "Libr@ry_ElMa27668";
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
                        $"http://194.146.242.26:7777/api/ForAllUsers/login?login={LoginTextBox.Text}&password={PasswordTextBox.Text}",
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
                        .GetMessageBoxStandard("Ошибка", $"Ошибка: Что-то пошло не так, проверьте логин или пароль",
                            ButtonEnum.Ok);

                    var result = await box.ShowAsync();
                }
            }
        }
        catch (Exception exception)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Ошибка", $"Ошибка: Что-то пошло не так, сервер не отвечает",
                    ButtonEnum.Ok);

            var result = await box.ShowAsync();
        }
        
            
    }
}