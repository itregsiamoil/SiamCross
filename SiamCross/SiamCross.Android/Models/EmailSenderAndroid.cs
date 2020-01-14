using Plugin.Messaging;
using SiamCross.Droid.Models;
using SiamCross.Models.Tools;
using System.IO;
using System.Net;
using System.Net.Mail;
using Xamarin.Forms;
using Settings = SiamCross.Models.Tools.Settings;
[assembly: Dependency(typeof(EmailSenderAndroid))]
namespace SiamCross.Droid.Models
{
    public class EmailSenderAndroid : IEmailSender
    {
        public void SendEmail(string to, string subject, string text)
        {
            
        }

        public void SendEmailWithFile(string filename)
        {
            var path = Path.Combine(
                        System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.ApplicationData), filename);

            if (!File.Exists(path)) return;

            MailAddress from = new MailAddress(Settings.Instance.FromAddress);
            MailAddress toMail = new MailAddress(Settings.Instance.ToAddress);
            MailMessage m = new MailMessage(from, toMail);
            m.Attachments.Add(new Attachment(path));
            m.Subject = "Mail with attachment";
            m.Body = "Measurement ";
            m.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                Settings.Instance.Port);
            smtp.Credentials = new NetworkCredential(Settings.Instance.Username,
                Settings.Instance.Password);
            smtp.EnableSsl = true;
            smtp.Send(m);
        }
    }
}