namespace Weatherer
{
    internal class Start
    {
        static async Task Main()
        {
            TelegramBot telegramBot = new TelegramBot();
            await telegramBot.Run(); //Запускаем бота
        }
    }
}

    