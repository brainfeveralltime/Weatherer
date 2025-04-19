using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Weatherer
{
    public class TelegramBot
    {
        private const string bot_token = "7761963577:AAE3xgNxPcY2lVftInKuUy59Kis7lANgwaQ";

        private const string api_key = "9801393a625e98a10027976638c32df7";

        private static TelegramBotClient bot;

        private string? City = null;

        private string Date = DateTime.Now.ToString().Split(' ').First();

        private string? Condition = null;

        public async Task Run()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            bot = new TelegramBotClient(bot_token, cancellationToken: cts.Token);

            var me = await bot.GetMe();

            bot.OnError += OnError;
            bot.OnMessage += OnMessage;
            bot.OnUpdate += OnUpdate;

            Console.WriteLine($"Бот @{me.Username} успешно запущен. Нажмите Escape для завершения");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            cts.Cancel();
        }

        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text is not { } text)
                return;
            else if (text.StartsWith('/'))
            {
                
                var space = text.IndexOf(' ');
                if (space < 0)
                    space = text.Length;
                var command = text[..space].ToLower();
                await OnCommand(command, text[space..].TrimStart(), message);

            }
            else
                await OnTextMessage(message);
        }
        async Task OnCommand(string command, string args, Message message)
        {
            Console.WriteLine($"Получена комманда: {command} {args}");
            switch (command)
            {
                case "/start":
                    await bot.SendMessage(message.Chat, 
                        "🌞 Приветствую, я <b>Weatherer</b> — ваш персональный метеоролог\n" + 
                        "Список моих комманд:\n" + 
                        "/weather [город] - Узнать актуальный прогноз погоды в определенном городе. Например, /weather Ростов-на-Дону\n" +
                        "/forecast [город] - Узнать прогноз погоды на несколько дней. Например, /forecast Москва\n" +
                        "/set [город] - Задать город по умолчанию. При вызове комманд /weather и /forecast без явного указания города для прогноза погоды будет использоваться город, заданный по умолчанию\n" +
                        "/remove - Убрать город, заданный по умолчанию",
                        ParseMode.Html, protectContent: true);
                    break;

                case "/weather":
                    var city = string.IsNullOrWhiteSpace(args) ? City : args;

                    if (string.IsNullOrWhiteSpace(city))
                    {
                        await bot.SendMessage(message.Chat, "Введите название города, чтобы получить прогноз погоды");
                        Condition = "weather";
                        return;
                    }

                    var weather = await GetWeather(city);
                    await bot.SendMessage(message.Chat, weather);
                    break;

                case "/set":
                    if (string.IsNullOrWhiteSpace(args))
                    {
                        await bot.SendMessage(message.Chat, "Введите город, который хотите задать по умолчанию");
                        Condition = "set";
                        return;
                    }

                    else
                    {
                        City = args;
                        await bot.SendMessage(message.Chat, $"Город по умолчанию установлен: {City}");
                    }
                    break;

                case "/remove":
                    City = null;
                    await bot.SendMessage(message.Chat, $"Город по умолчанию убран");
                    break;

                default:
                    await bot.SendMessage(message.Chat, "Команда не найдена");
                    break;
            }
        }

        async Task OnTextMessage(Message message) 
        {
            if (string.IsNullOrWhiteSpace(message.Text))
                return;

            Console.WriteLine($"Получено сообщение \"{ message.Text}\" в {message.Chat}");
            switch (Condition)
            {
                case "weather":
                    var weather = await GetWeather(message.Text);
                    await bot.SendMessage(message.Chat, weather);
                    break;

                case "set":
                    await OnCommand("/set", message.Text, message);
                    break;

                default:
                    await OnCommand("/start", "", message);
                    break;
            }
            Condition = null;
        }
        async Task OnUpdate(Update update)
        {

        }

        public async Task<string> GetWeather(string city)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={api_key}&units=metric&lang=ru";

            using var client = new HttpClient();

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return $"Ошибка при запросе: {response.StatusCode}";

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var weather = JsonSerializer.Deserialize<WeatherApiResponse>(json, options);

            if (weather == null || weather.Weather.Length == 0)
                return "Не удалось получить информацию о погоде";


            return $"Погода в городе {weather.Name}:\n" +
                   $"📅 Дата: {Date}\n" +
                   $"🌡 Температура: {weather.Main.Temp}°C\n" +
                   $"💧 Влажность: {weather.Main.Humidity}%\n" +
                   $"💨 Ветер: {weather.Wind.Speed} м/с\n" + 
                   $"🌤 Описание: {weather.Weather[0].Description}";
        }

       
    }
}

    



