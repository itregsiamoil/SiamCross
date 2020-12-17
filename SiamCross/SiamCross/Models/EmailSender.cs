using MailKit.Net.Smtp;
using MimeKit;
using SiamCross.Models.Tools;
using System;
using System.Threading.Tasks;
using Settings = SiamCross.Models.Tools.Settings;

namespace SiamCross.Models
{
    public class EmailSender : IEmailSender
    {
        async Task<bool> SendMessage(MimeMessage msg)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(Settings.Instance.SmtpAddress, Settings.Instance.Port);

                    if (Settings.Instance.IsNeedAuthorization)
                    {
                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        //client.AuthenticationMechanisms.Remove("XOAUTH2");
                        client.Authenticate(Settings.Instance.Username, Settings.Instance.Password);
                    }
                    await client.SendAsync(msg);
                    client.Disconnect(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> SendEmailWithFiles(string subject, 
                                       string text, 
                                       string[] filenames)
        {
            var m = new MimeMessage();

            MailboxAddress sender = new MailboxAddress("SiamService", Settings.Instance.FromAddress);
            MailboxAddress receiver = new MailboxAddress("", Settings.Instance.ToAddress);
            m.From.Add(sender);
            m.To.Add(receiver);
            m.Subject = subject;

            var builder = new BodyBuilder();
            builder.TextBody = text;

            foreach (var path in filenames)
            {
                builder.Attachments.Add(path);
            }
            m.Body = builder.ToMessageBody();
            return await SendMessage(m);
        }
    }
}