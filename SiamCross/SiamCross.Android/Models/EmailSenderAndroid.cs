using SiamCross.Droid.Models;
using SiamCross.Models.Tools;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Xamarin.Forms;
using Settings = SiamCross.Models.Tools.Settings;
[assembly: Dependency(typeof(EmailSenderAndroid))]
namespace SiamCross.Droid.Models
{
    public class EmailSenderAndroid : IEmailSender
    {
        public Task SendEmail(string to, string subject, string text)
        {
            throw new NotImplementedException();
        }

        public Task SendEmailWithFile(string filename)
        {
            return new Task(() =>
            {
                var path = Path.Combine(
                        Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), filename);

                if (File.Exists(path))
                {
                    var from = new MailAddress(Settings.Instance.FromAddress);
                    var toMail = new MailAddress(Settings.Instance.ToAddress);
                    var m = new MailMessage(from, toMail);
                    m.Attachments.Add(new Attachment(path));
                    m.Subject = "Mail with attachment";
                    m.Body = "Measurement ";
                    m.IsBodyHtml = true;
                    var smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                        Settings.Instance.Port)
                    {
                        Credentials = new NetworkCredential(Settings.Instance.Username,
                        Settings.Instance.Password),
                        EnableSsl = true
                    };
                    smtp.Send(m);
                }
            });
        }
    }
}