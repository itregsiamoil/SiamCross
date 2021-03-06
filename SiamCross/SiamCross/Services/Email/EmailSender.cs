﻿using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Services.Email
{
    [Preserve(AllMembers = true)]
    internal class EmailSender
    {
        private async Task<bool> SendMessage(MimeMessage msg)
        {
            try
            {
                //var dir = EnvironmentService.Instance.GetDir_Downloads();
                //var path = Path.Combine(dir, "smtp.log");
                //using (SmtpClient client = new SmtpClient(new ProtocolLogger(path)))
                using (SmtpClient client = new SmtpClient())
                {
                    var settings = await Repo.MailSettingsDir.GetData();

                    await client.ConnectAsync(settings.SmtpAddress, settings.Port);

                    if (settings.NeedAuthorization)
                    {
                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        //client.AuthenticationMechanisms.Remove("XOAUTH2");
                        await client.AuthenticateAsync(settings.Username, settings.Password);
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
                                       IReadOnlyCollection<string> filenames)
        {
            MimeMessage m = new MimeMessage();

            var settings = await Repo.MailSettingsDir.GetData();

            MailboxAddress sender = new MailboxAddress(settings.FromName, settings.FromAddress);
            MailboxAddress receiver = new MailboxAddress(string.Empty, settings.ToAddress);
            m.From.Add(sender);
            m.To.Add(receiver);
            m.Subject = subject;

            //BodyBuilder builder = new BodyBuilder
            //{
            //    TextBody = text
            //};
            BodyBuilder builder = new BodyBuilder();

            foreach (string path in filenames)
            {
                //builder.Attachments.Add(path, path);
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    string name = Path.GetFileName(path);
                    byte[] b = ParserOptions.Default.CharsetEncoding.GetBytes(name.ToCharArray());
                    name = Encoding.ASCII.GetString(b);
                    await builder.Attachments.AddAsync(name, fs, ContentType.Parse("application/octet-stream"));
                    fs.Close();
                }
            }
            m.Body = builder.ToMessageBody();
            return await SendMessage(m);
        }
    }
}