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
        
        private static TelegramBotClient bot;

        public async Task Run()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();

            bot = new TelegramBotClient(bot_token, cancellationToken: cts.Token);

            var me = await bot.GetMe();

            bot.OnError += OnError;
            bot.OnMessage += OnMessage;
            bot.OnUpdate += OnUpdate;

            Console.WriteLine($"Бот @{me.Username} успешно запущен. Нажмите enter для завершения");
            Console.ReadLine();
            cts.Cancel();
        }

        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text == "/start")
            {
                await bot.SendMessage(message.Chat, "🌞 Приветствую, я <b>Weatherer</b> — ваш персональный метеоролог\nВыберите подходящий пункт в контекстном меню или введите одну из комманд:\n/weather [город] - Узнать актуальный прогноз погоды в определенном городе. Например, /weather Ростов-на-Дону",
                    ParseMode.Html,
                    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("🌦 Узнать погоду", "weather_request")));

            }
        }

        async Task OnUpdate(Update update)
        {
            if (update is { CallbackQuery: { } query })
            {
                if (query.Data == "weather_request")
                    await bot.SendMessage(query.Message!.Chat, "Отлично! Введите название города, чтобы узнать самый лучший прогноз погоды :)");
            }
            
        }

    }

}

