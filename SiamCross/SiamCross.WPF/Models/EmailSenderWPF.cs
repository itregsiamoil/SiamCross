using SiamCross.Models.Tools;
using SiamCross.WPF.Models;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(EmailSenderWPF))]
namespace SiamCross.WPF.Models
{
    public class EmailSenderWPF : IEmailSender
    {
        public void SendEmail(string to, string subject, string text)
        {
            //return new Task(() =>
            //{
                MailAddress from = new MailAddress(Settings.Instance.FromAddress);
                MailAddress toMail = new MailAddress(Settings.Instance.ToAddress);
                MailMessage m = new MailMessage(from, toMail);
                m.Subject = subject;
                m.Body = text;
                m.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                    Settings.Instance.Port);
                smtp.Credentials = new NetworkCredential(Settings.Instance.Username,
                    Settings.Instance.Password);
                smtp.EnableSsl = true;
                smtp.Send(m);
                //smtp.SendAsync(m, null);
            //});
        }

        public void SendEmailWithFile(string filename)
        {
            //return new Task(() =>
            //{
                var path = Path.Combine(Directory.GetCurrentDirectory(), filename);

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
            //});
        }

        public void SendEmailWithFiles(string subject, string text, string[] filenames)
        {
            throw new System.NotImplementedException();
        }
    }
}
