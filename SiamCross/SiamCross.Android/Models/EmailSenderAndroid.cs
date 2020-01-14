using Plugin.Messaging;
using SiamCross.Droid.Models;
using SiamCross.Models.Tools;
using System.IO;
using System.Net;
using System.Net.Mail;
using Xamarin.Forms;

[assembly: Dependency(typeof(EmailSenderAndroid))]
namespace SiamCross.Droid.Models
{
    public class EmailSenderAndroid : IEmailSender
    {
        public void SendEmail(string to, string subject, string text)
        {
            MailAddress from = new MailAddress("gelcen777@gmail.com");
            MailAddress toMail = new MailAddress(to);
            MailMessage m = new MailMessage(from, toMail);
            m.Subject = subject;
            m.Body = text;
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("gelcen777@gmail.com", "");
            smtp.EnableSsl = true;
            smtp.Send(m);
        }

            public void SendEmailWithFile(string filename)
        {
            var file = Path.Combine(
                        System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), filename);

            if (file != null && File.Exists(file))
            {
                var emailMessenger =
                    CrossMessaging.Current.EmailMessenger;

                if (emailMessenger.CanSendEmail)
                {
                    var email = new EmailMessageBuilder()
                    .To("gelcen777@gmail.com")
                    .Subject("Xamarin Messaging Plugin")
                    .Body("Well hello there from Xam.Messaging.Plugin")
                    .WithAttachment(file, "measurement")
                    .Build();
                    emailMessenger.SendEmail(email);
                }
            }
        }
    }
}