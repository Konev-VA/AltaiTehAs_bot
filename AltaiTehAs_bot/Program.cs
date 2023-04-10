using Telegram.Bot;
using Telegram.Bot.Polling;

namespace AltaiTehAs_bot
{
    internal class Program
    {
        static ITelegramBotClient altaiTehAsBot = new TelegramBotClient("6000456363:AAEzXL4QZwK4p8UkQoW3Sk0_I9LhG13vapg");

        static void Main(string[] args)
        {
            var cancellationToken = new CancellationTokenSource().Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            var temp = new UpdateHandler();

            altaiTehAsBot.StartReceiving(
                temp.HandleUpdateAsync,
                temp.HandlePollingErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.WriteLine($"Запущено в {DateTime.Now}");

            Console.ReadLine();
        }
    }
}