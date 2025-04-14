namespace Weatherer;

    public static class Start
    {
        public static async Task Main()
        {
            TelegramBot telegramBot = new TelegramBot();
            await telegramBot.Run();
        }
    }