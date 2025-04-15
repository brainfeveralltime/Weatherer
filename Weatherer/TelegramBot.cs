using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Weatherer.JsonClasses;

namespace Weatherer
{
    public class TelegramBot
    {
        private const string bot_token = "7761963577:AAE3xgNxPcY2lVftInKuUy59Kis7lANgwaQ";

        public const string api_key = "9801393a625e98a10027976638c32df7";

        private static TelegramBotClient bot;

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
            Console.WriteLine($"Received command: {command} {args}");
            switch (command)
            {
                case "/start":
                    await bot.SendMessage(message.Chat, "🌞 Приветствую, я <b>Weatherer</b> — ваш персональный метеоролог\nСписок моих комманд:\n/weather [город] - Узнать актуальный прогноз погоды в определенном городе. Например, /weather Ростов-на-Дону",
                        ParseMode.Html, protectContent: true);
                    break;
                case "/weather":
                    var weather = await GetWeather(args);
                    await bot.SendMessage(message.Chat, weather);
                    break;
                default:
                    await bot.SendMessage(message.Chat, "К сожалению, такой команды у меня нет");
                    break;
            }
        }

        async Task OnTextMessage(Message message) 
        {
            Console.WriteLine($"Received text '{message.Text}' in {message.Chat}");
            await OnCommand("/start", "", message); 
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

            var weather = JsonSerializer.Deserialize<WeatherApiResponse>(json);

            if (weather == null || weather.Weather.Length == 0)
                return "Не удалось получить информацию о погоде.";

            
            string emoji = GetWeatherEmoji(weather.Weather[0].Id);

            return $"{emoji} Погода в городе {weather.Name}:\n" +
                   $"🌡 Температура: {weather.Main.Temp}°C\n" +
                   $"💧 Влажность: {weather.Main.Humidity}%\n" +
                   $"💨 Ветер: {weather.Wind.Speed} м/с\n" +
                   $"🌤 Описание: {weather.Weather[0].Description}";
        }

        private string GetWeatherEmoji(int id)
        {
            
            if (id >= 200 && id < 300) return "⛈";
            if (id >= 300 && id < 400) return "🌦";
            if (id >= 500 && id < 600) return "🌧";
            if (id >= 600 && id < 700) return "❄";
            if (id >= 700 && id < 800) return "🌫";
            if (id == 800) return "☀";
            if (id > 800) return "☁";
            return "🌈";
        }
    }
}

    



