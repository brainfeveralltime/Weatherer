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
        private const string Bot_token = "7761963577:AAE3xgNxPcY2lVftInKuUy59Kis7lANgwaQ";

        private const string Api_key = "9801393a625e98a10027976638c32df7";

        private static TelegramBotClient bot;

        private long ChatId;

        private string? City;

        private DateOnly Date = DateOnly.FromDateTime(DateTime.Now);

        private bool Following;

        private TimeOnly Follow_Time;

        private string? Condition;

        public async Task Run()
        {
            using var cts = new CancellationTokenSource();

            bot = new TelegramBotClient(Bot_token, cancellationToken: cts.Token);

            var me = await bot.GetMe();

            bot.OnError += OnError;
            bot.OnMessage += OnMessage;

            Console.WriteLine($"Бот @{me.Username} успешно запущен. Нажмите Escape для завершения");

            _ = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (Following == true)
                    {
                        await Subscription();
                        await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
                    }
                }
            });

            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            cts.Cancel();
        }

        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        async Task OnMessage(Message message, UpdateType type)
        {
            ChatId = message.Chat.Id;

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
                    await bot.SendMessage(ChatId,
                        "☀️ Приветствую, я <b>Weatherer</b> — ваш персональный метеоролог.\n" +
                        "📋 Список моих комманд:\n" +
                        "🌥️ Узнать погоду в в любом городе: /weather [город]\n" +
                        "🏙️ Задать город по умолчанию. В этом случае, если не указать город, для прогноза погоды будет использоваться город, заданный по умолчанию: /set [город]\n" +
                        "🚫 Убрать город, заданный по умолчанию: /remove\n" +
                        "🔔 Подписаться на рассылку погоды в заданное время (необходимо перед этим задать город по умолчанию): /follow [время]\n" +
                        "🔕 Отписаться от рассылки: /unfollow\n",
                        ParseMode.Html, 
                        replyMarkup: new KeyboardButton[][] 
                        {
                            ["/start", "/weather"],
                            ["/set", "/remove"],
                            ["/follow", "/unfollow"],
                        });

                    break;

                case "/weather":
                    var city = string.IsNullOrWhiteSpace(args) ? City : args;

                    if (string.IsNullOrWhiteSpace(city))
                    {
                        await bot.SendMessage(ChatId, "📌 Введите название города, чтобы получить прогноз погоды");
                        Condition = "weather";
                        return;
                    }

                    var weather = await GetWeather(city);
                    await bot.SendMessage(ChatId, weather);
                    break;

                case "/set":
                    if (string.IsNullOrWhiteSpace(args))
                    {
                        await bot.SendMessage(ChatId, "📌 Введите город, который хотите задать по умолчанию");
                        Condition = "set";
                        return;
                    }

                    City = args;
                    await bot.SendMessage(ChatId, $"✅ Город по умолчанию установлен: {City}");

                    break;

                case "/remove":
                    City = null;
                    await bot.SendMessage(ChatId, $"✅ Город по умолчанию убран");
                    break;

                case "/follow":
                    if (string.IsNullOrWhiteSpace(args) || !TimeOnly.TryParse(args, out var time))
                    {
                        await bot.SendMessage(ChatId, "📌 Введите дату в формате hh:mm");
                        Condition = "follow";
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(City))
                    {
                        await bot.SendMessage(ChatId, "📌 Сначала задайте город по умолчанию с помощью /set");
                        return;
                    }

                    await bot.SendMessage(ChatId,
                            "✅ Вы подписались на рассылку\n" +
                            $"Теперь вы будете получать актуальный прогноз погоды в городе {City} каждый день в {time}!");
                    Following = true;
                    Follow_Time = time;

                    break;

                case "/unfollow":
                    Following = false;
                    await bot.SendMessage(ChatId, $"✅ Вы отписались от рассылки");
                    break;

                default:
                    await bot.SendMessage(ChatId, "⚠️ Команда не найдена");
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
                    Condition = null;
                    await OnCommand("/weather", message.Text, message);
                    break;

                case "set":
                    Condition = null;
                    await OnCommand("/set", message.Text, message);
                    break;

                case "follow":
                    Condition = null;
                    await OnCommand("/follow", message.Text, message);
                    break;

                default:
                    Condition = null;
                    await OnCommand("/start", "", message);
                    break;
            }
            
        }

        

        async Task<string> GetWeather(string city)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={Api_key}&units=metric&lang=ru";

            using var client = new HttpClient();

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return $"⚠️ Ошибка при запросе: {response.StatusCode}";

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };

            var weather = JsonSerializer.Deserialize<WeatherApiResponse>(json, options);

            if (weather == null || weather.Weather.Length == 0)
                return "❌ Не удалось получить информацию о погоде";


            return $"🌆 Погода в городе {weather.Name}:\n" +
                   $"🗓️ Дата: {Date}\n" +
                   $"🌡 Температура: {weather.Main.Temp}°C\n" +
                   $"💧 Влажность: {weather.Main.Humidity}%\n" +
                   $"💨 Ветер: {weather.Wind.Speed} м/с\n" + 
                   $"🌤 Описание: {weather.Weather[0].Description}";
        }

        async Task Subscription()
        {
            if (string.IsNullOrWhiteSpace(City))
                return;

            var cur_time = TimeOnly.FromDateTime(DateTime.Now);
            if (cur_time.Hour == Follow_Time.Hour && cur_time.Minute == Follow_Time.Minute)
            {
                await bot.SendMessage(ChatId, "🔔 Погода по подписке!\n");

                var weather = await GetWeather(City);
                await bot.SendMessage(ChatId, weather);
                
                
            }
        }
       
    }
}

    



