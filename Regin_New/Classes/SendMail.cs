using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Regin_New.Classes
{
    public class SendMail
    {
        public static void SendMessage(string Message, string To)
        {
            var smtpClient = new SmtpClient("smtp.yandex.ru")
            {
                Port = 587,
                Credentials = new NetworkCredential("msvlls@yandex.ru", "vitfdmtkwzsmssjp"),
                EnableSsl = true,
            };

            smtpClient.Send("msvlls@yandex.ru", To, "RegIN_6pr", Message);
        }
    }
}
