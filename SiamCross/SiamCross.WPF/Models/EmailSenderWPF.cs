using Plugin.Messaging;
using SiamCross.Models.Tools;
using SiamCross.WPF.Models;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(EmailSenderWPF))]
namespace SiamCross.WPF.Models
{
    public class EmailSenderWPF : IEmailSender
    {
        public void SendEmail(string to, string subject, string text)
        {
            throw new System.NotImplementedException();
        }

        public void SendEmailWithFile(string filename)
        {
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), filename);

            if (File.Exists(path))
            {
                var emailMessenger =
                    CrossMessaging.Current.EmailMessenger;

                if (emailMessenger.CanSendEmail)
                {
                    var email = new EmailMessageBuilder()
                    .To("gelcen777@gmail.com")
                    .Subject("Xamarin Messaging Plugin")
                    .Body("Well hello there from Xam.Messaging.Plugin")
                    .WithAttachment(path, "measurement")
                    .Build();

                    emailMessenger.SendEmail(email);
                }
            }
        }
    }
}
