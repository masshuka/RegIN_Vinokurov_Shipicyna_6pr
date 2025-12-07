using System.Net.Mail;
using System.Net;

namespace Regin_New.Classes
{
    public static class SendMail
    {
        private static readonly SmtpClient smtpClient = new("smtp.yandex.ru")
        {
            Port = 587,
            Credentials = new NetworkCredential("msvlls@yandex.ru", "vitfdmtkwzsmssjp"),
            EnableSsl = true
        };

        private const string FromEmail = "msvlls@yandex.ru";
        private const string Subject = "RegIN_6pr";

        public static void SendMessage(string message, string to)
        {
            smtpClient.Send(FromEmail, to, Subject, message);
        }
    }
}
