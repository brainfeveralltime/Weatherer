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

        private long? ChatId = null;

        private string? City = null;

        private DateOnly Date = DateOnly.FromDateTime(DateTime.Now);

        private TimeOnly Follow_Time = TimeOnly.FromDateTime(DateTime.Now);

        private string? Condition = null;

        public async Task Run()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            bot = new TelegramBotClient(Bot_token, cancellationToken: cts.Token);

            var me = await bot.GetMe();

            bot.OnError += OnError;
            bot.OnMessage += OnMessage;
            bot.OnUpdate += OnUpdate;

            Console.WriteLine($"Бот @{me.Username} успешно запущен. Нажмите Escape для завершения");

            _ = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await Subscription();
                    await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);
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
                        "🌞 Приветствую, я <b>Weatherer</b> — ваш персональный метеоролог.\n" + 
                        "Список моих комманд:\n" +
                        "1️⃣ /weather [город] - Узнать актуальный прогноз погоды в определенном городе\n" +
                        "2️⃣ /set [город] - Задать город по умолчанию. При вызове комманды /weather без явного указания города, для прогноза погоды будет использоваться город, заданный по умолчанию\n" +
                        "3️⃣ /remove - Убрать город, заданный по умолчанию\n" +
                        "4️⃣ /follow [время] - подписаться на рассылку погоды в заданное время. Необходимо перед этим задать город командой /set",
                        ParseMode.Html, 
                        replyMarkup: new InlineKeyboardButton[][] 
                        {
                            [("Узнать погоду", "weather")],
                            [("Задать город", "set")],
                            [("Убрать город", "remove")]
                        } );
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
                        await bot.SendMessage(message.Chat, $"✔️ Город по умолчанию установлен: {City}");
                    }
                    break;

                case "/remove":
                    City = null;
                    await bot.SendMessage(message.Chat, $"✔️ Город по умолчанию убран");
                    break;

                case "/follow":
                    if (string.IsNullOrWhiteSpace(args) || !TimeOnly.TryParse(args, out var time))
                    {
                        await bot.SendMessage(message.Chat, "Введите дату в формате hh:mm");
                        Condition = "follow";
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(City))
                    {
                        await bot.SendMessage(message.Chat, "Сначала задайте город по умолчанию с помощью /set");
                        return;
                    }

                    else
                    {
                        await bot.SendMessage(message.Chat, 
                            "✔️ Вы подписались на рассылку\n" +
                            $"Теперь вы будете получать актуальный прогноз погоды в городе {City} каждый день в {time}!");
                        Follow_Time = time;
                    }
                    break;

                default:
                    await bot.SendMessage(message.Chat, "⚠️ Команда не найдена");
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
                    await OnCommand("/weather", message.Text, message);
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
            if (update is { CallbackQuery: { } query }) 
            {
                Console.WriteLine(query.Data);
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


            return $"Погода в городе {weather.Name}:\n" +
                   $"📅 Дата: {Date}\n" +
                   $"🌡 Температура: {weather.Main.Temp}°C\n" +
                   $"💧 Влажность: {weather.Main.Humidity}%\n" +
                   $"💨 Ветер: {weather.Wind.Speed} м/с\n" + 
                   $"🌤 Описание: {weather.Weather[0].Description}";
        }

        async Task Subscription()
        {
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

    



