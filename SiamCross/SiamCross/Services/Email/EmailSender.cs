using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Settings = SiamCross.Models.Tools.Settings;

namespace SiamCross.Services.Email
{
    [Preserve(AllMembers = true)]
    internal class EmailSender
    {
        private async Task<bool> SendMessage(MimeMessage msg)
        {
            try
            {
                using (SmtpClient client = new SmtpClient())
                {
                    await client.ConnectAsync(Settings.Instance.SmtpAddress, Settings.Instance.Port);

                    if (Settings.Instance.IsNeedAuthorization)
                    {
                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        //client.AuthenticationMechanisms.Remove("XOAUTH2");
                        await client.AuthenticateAsync(Settings.Instance.Username, Settings.Instance.Password);
                    }
                    await client.SendAsync(msg);
                    await client.DisconnectAsync(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> SendEmailWithFilesAsync(string subject,
                                       string text,
                                       string[] filenames)
        {
            MimeMessage m = new MimeMessage();

            MailboxAddress sender = new MailboxAddress("SiamService", Settings.Instance.FromAddress);
            MailboxAddress receiver = new MailboxAddress("", Settings.Instance.ToAddress);
            m.From.Add(sender);
            m.To.Add(receiver);
            m.Subject = subject;

            BodyBuilder builder = new BodyBuilder
            {
                TextBody = text
            };

            foreach (string path in filenames)
            {
                builder.Attachments.Add(path);
            }
            m.Body = builder.ToMessageBody();
            return await SendMessage(m);
        }
    }
}