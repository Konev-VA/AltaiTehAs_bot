using AltaiTehAs_bot.DAL;
using AltaiTehAs_bot.Models;
using System.Net.Mail;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Xml.Xsl;
using Newtonsoft.Json.Serialization;

namespace AltaiTehAs_bot
{
    public class UpdateHandler : IUpdateHandler
    {
        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            return;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Message? outMessage = null;

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;

                await new MessageDAL().CreateMyMessage(new MyMessage()
                {
                    MessageId = message.MessageId,
                    Text = message.Text,
                    UserId = message.From.Id,
                    Date = message.Date
                });

                if (message.Text == "/start")
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Ремонт", "Repair"),
                            InlineKeyboardButton.WithCallbackData("Консультация", "Consultation")
                        },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Как добраться","Route"),
                            InlineKeyboardButton.WithCallbackData("Контакты","Contacts"),
                            InlineKeyboardButton.WithCallbackData("О нас","About")
                        }
                    });

                    outMessage = await botClient.SendTextMessageAsync(message.Chat,
                                                         "Приветствую! Я бот для регистрации заявок на ремонт с/х техники. \n Выберите, с чем связан ваш вопрос.",
                                                         replyMarkup: inlineKeyboard);

                    return;
                }

                if (message.From.Id == 831058266 && message.Text.ToLower().Equals("заявки"))
                {
                    var consultations = await new ConsultationDAL().GetConsultations();

                    var repairs = await new RepairDal().GetRepairs();

                    string result = "";

                    result += "Консультации: ";

                    foreach (var consultation in consultations)
                    {
                        var user = await new UserDAL().GetUserById(consultation.UserId);

                        result += "\n\n";
                        result += $"{consultation.Question}. \n({user.Phone} - {user.Name})";
                    }

                    result += "\n\n\nРемонт: ";

                    foreach (var repair in repairs)
                    {
                        var user = await new UserDAL().GetUserById(repair.UserId);

                        result += "\n\n";
                        result += $"Тип техники: {repair.TechType}. \nСуть обращения: {repair.Description}. \n({user.Phone} - {user.Name})";
                    }

                    await botClient.SendTextMessageAsync(message.Chat, result);
                }

                if (message.Type == MessageType.Contact)
                {
                    var contact = message.Contact;

                    var userDAL = new UserDAL();

                    var curUser = await userDAL.GetUserById((long)contact.UserId);

                    if (curUser != null)
                    {
                        outMessage = await botClient.SendTextMessageAsync(message.Chat, "Ваш номер нам уже известен", replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        await userDAL.CreateUser(new Models.User()
                        {
                            UserId = (long)contact.UserId,
                            Name = $"{contact.LastName} {contact.FirstName}",
                            Phone = contact.PhoneNumber
                        });

                        outMessage = await botClient.SendTextMessageAsync(message.Chat, "Спасибо, что оставили ваши контакты. \r\nВ ближайшее время с вами свяжется специалист для подробной консультации", replyMarkup: new ReplyKeyboardRemove());
                    }

                    var lastCons = await new ConsultationDAL().GetLastConsultation(message.From.Id);

                    var lastRepair = await new RepairDal().GetLastRepair(message.From.Id);

                    if (lastCons.CreationDate > lastRepair.CreationDate)
                    {
                        await NotifyConsultation(lastCons, botClient);
                        return;
                    }

                    if (lastCons.CreationDate < lastRepair.CreationDate)
                    {
                        await NotifyRepair(lastRepair, botClient);
                        return;
                    }


                    await NotifyRepair(lastRepair, botClient);
                    await NotifyConsultation(lastCons, botClient);
                    return;
                }

                var lastMessage = await new MessageDAL().GetMyMessageById(message.MessageId - 1, message.From.Id);

                if (lastMessage != null && lastMessage.Text != null)
                {

                    if (lastMessage.Text.Contains("вопрос"))
                    {
                        var consDAL = new ConsultationDAL();

                        var consultation = await consDAL.GetLastConsultation(message.From.Id);

                        consultation.Question = message.Text;

                        await consDAL.UpdateConsultation(consultation);

                        outMessage = await HandleUserAsync(message, botClient);

                        if (outMessage.Text.Contains("контакт"))
                            return;

                        await NotifyConsultation(consultation, botClient);
                    }

                    if (lastMessage.Text.Contains("техники"))
                    {
                        var repairDAL = new RepairDal();

                        var repair = await repairDAL.GetLastRepair(message.From.Id);

                        repair.TechType = message.Text;


                        await repairDAL.UpdateRepair(repair);

                        outMessage = await botClient.SendTextMessageAsync(message.Chat, "Опишите, какой вид ремонта необходимо выполнить");
                    }

                    if (lastMessage.Text.Contains("ремонта"))
                    {
                        var repairDAL = new RepairDal();

                        var repair = await repairDAL.GetLastRepair(message.From.Id);

                        repair.Description = message.Text;

                        await repairDAL.UpdateRepair(repair);

                        outMessage = await HandleUserAsync(message, botClient);

                        if (outMessage.Text.Contains("контакт"))
                            return;

                        await NotifyRepair(repair, botClient);
                    }
                }

                if (outMessage != null)
                    await new MessageDAL().CreateMyMessage(new MyMessage()
                    {
                        MessageId = outMessage.MessageId,
                        Text = outMessage.Text,
                        UserId = update.Message.From.Id,
                        Date = outMessage.Date
                    });
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                var message = update.CallbackQuery.Message;

                await new MessageDAL().CreateMyMessage(new MyMessage()
                {
                    MessageId = message.MessageId,
                    Text = message.Text,
                    UserId = message.From.Id,
                    Date = message.Date
                });
                var chat = update.CallbackQuery.Message.Chat;

                var data = update.CallbackQuery.Data;

                if (data == "Repair")
                {
                    await new RepairDal().CreateRepair(new Repair()
                    {
                        UserId = update.CallbackQuery.From.Id,
                        CreationDate = DateTime.Now
                    });

                    outMessage = await botClient.SendTextMessageAsync(chat, "Введите вид с\\х техники");
                }

                if (data == "Consultation")
                {
                    await new ConsultationDAL().CreateConsultation(new Consultation()
                    {
                        UserId = update.CallbackQuery.From.Id,
                        CreationDate = DateTime.Now
                    });

                    outMessage = await botClient.SendTextMessageAsync(chat, "Расскажите, с чем связан ваш вопрос?");
                }

                if (data == "Route")
                {
                    outMessage = await botClient.SendTextMessageAsync(
                        chat,
                        "Мы находимся по адресу г.Барнаул, ул. Новосибирская, д. 44е.\r\nБудем рады вашему визиту!");

                    await botClient.SendVenueAsync(
                        chat,
                        latitude: 53.306603,
                        longitude: 83.608298,
                        title: "Алтай Тех Ас",
                        address: "Барнаул, ул. Новосибирская 44Е");
                }

                if (data == "Contacts")
                    outMessage = await botClient.SendTextMessageAsync(chat,
                                                         "Телефон директора: +7(999)999-99-99 \r\nemail: altai_teh_as_help@pochta.domen \r\nАдрес: Барнаул, ул. Новосибирская 44Е");

                if (data == "About")
                    outMessage = await botClient.SendTextMessageAsync(chat,
                                                         "Компания «АлтайТехАс плюс» была создан в августе 2019 года.\r\nФирма занимается диагностикой, ремонтом, и плановым обслуживанием \r\nтакой техникой как грузовики, комбайны, погрузчики, трактора и прочей сельскохозяйственной техники.\r\nНашим преимуществом перед другими компаниями в этой области является то, что перечисленные услуги могут \r\nбыть оказаны с выездом на территорию клиента.");


                await new MessageDAL().CreateMyMessage(new MyMessage()
                {
                    MessageId = outMessage.MessageId,
                    Text = outMessage.Text,
                    UserId = update.CallbackQuery.From.Id,
                    Date = outMessage.Date
                });
            }
        }

        private async Task<Message> HandleUserAsync(Message message, ITelegramBotClient botClient)
        {
            var userDAL = new UserDAL();

            if (await userDAL.GetUserById(message.From.Id) == null)
                return await botClient.SendTextMessageAsync(
                    message.Chat,
                    "Ваша заявка зафиксирована. Для обратной связи рекомендуем оставить нам ваш контакт",
                    replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("Отправить номер") { RequestContact = true }));
            else
                return await botClient.SendTextMessageAsync(
                        message.Chat,
                        "Ваша заявка зафиксирована. Ожидайте ответ");
        }

        private async Task NotifyRepair(Repair repair, ITelegramBotClient botClient)
        {
            var userDAL = new UserDAL();

            var user = await userDAL.GetUserById(repair.UserId);

            await botClient.SendTextMessageAsync(831058266, $"Появилась новая заявка на ремонт. \r\nВид техники: {repair.TechType}\r\nСуть заявки: {repair.Description}\r\nКонтакты: {(user == null ? "не предоставлены" : $"{user.Phone} ({user.Name})")}");

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                //Логин и пароль должны быть созданы в настройках политики безопасности для каждого приложения отдельно
                Credentials = new NetworkCredential("AltaiTehAs", "qcytekztovfamfqz"),
                EnableSsl = true,
                UseDefaultCredentials = false
            };

            smtpClient.Send(
                from: "altaitehas@gmail.com",
                recipients: "makogon678@mail.ru",
                subject: "Новая заявка",
                body: $"Появилась новая заявка на ремонт. \r\nВид техники: {repair.TechType}\r\nСуть заявки: {repair.Description}\r\nКонтакты: {(user == null ? "не предоставлены" : $"{user.Phone} ({user.Name})")}");
        }

        private async Task NotifyConsultation(Consultation consultation, ITelegramBotClient botClient)
        {
            var userDAL = new UserDAL();

            var user = await userDAL.GetUserById(consultation.UserId);

            await botClient.SendTextMessageAsync(831058266, $"Появилась новая заявка на консультацию. \r\nСуть заявки: {consultation.Question}\r\nКонтакты: {(user == null ? "не предоставлены" : $"{user.Phone} ({user.Name})")}");
        }
    }
}
