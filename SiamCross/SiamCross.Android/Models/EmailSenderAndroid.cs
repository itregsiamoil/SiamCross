//using MailKit.Net.Smtp;
//using MimeKit;
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
        public void SendEmail(string to, 
                              string subject, 
                              string text)
        {
            try
            {
                var from = new MailAddress(Settings.Instance.FromAddress);
                var toMail = new MailAddress(Settings.Instance.ToAddress);
                var m = new MailMessage(from, toMail);
                m.Subject = subject;
                m.Body = text;
                m.IsBodyHtml = true;
                SmtpClient smtp = null;

                if (Settings.Instance.NeedAuthorization)
                {

                    smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                                          Settings.Instance.Port)
                    {
                        Credentials = new NetworkCredential(Settings.Instance.Username,
                                                            Settings.Instance.Password),
                        EnableSsl = true
                    };
                }
                else
                {
                    smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                                          Settings.Instance.Port)
                    {
                        EnableSsl = false
                    };
                }
                
                smtp?.Send(m);

                #region С использованием MailKit
                //var message = new MimeMessage();
                //message.From.Add(new MailboxAddress("Joey Tribbiani", Settings.Instance.FromAddress));
                //message.To.Add(new MailboxAddress("Mrs. Chanandler Bong", Settings.Instance.ToAddress));
                //message.Subject = subject;

                //message.Body = new TextPart("plain")
                //{
                //    Text = text
                //};

                //using (var client = new SmtpClient())
                //{
                //    client.Connect(Settings.Instance.SmtpAddress, Settings.Instance.Port);


                //    // Note: since we don't have an OAuth2 token, disable
                //    // the XOAUTH2 authentication mechanism.
                //   // client.AuthenticationMechanisms.Remove("XOAUTH2");

                //    // Note: only needed if the SMTP server requires authentication
                //    client.Authenticate(Settings.Instance.Username, Settings.Instance.Password);

                //    client.Send(message);
                //    client.Disconnect(true);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendEmailWithFile(string path)
        {
            if (File.Exists(path))
            {
                var from = new MailAddress(Settings.Instance.FromAddress);
                var toMail = new MailAddress(Settings.Instance.ToAddress);
                var m = new MailMessage(from, toMail);
                m.Attachments.Add(new Attachment(path));
                m.Subject = "Mail with attachment";
                m.Body = "Measurement ";
                m.IsBodyHtml = true;
                SmtpClient smtp = null;

                if (Settings.Instance.NeedAuthorization)
                {
                    smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                                          Settings.Instance.Port)
                    {
                        Credentials = new NetworkCredential(Settings.Instance.Username,
                                                            Settings.Instance.Password),
                        EnableSsl = true
                    };
                }
                else
                {
                    smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                                          Settings.Instance.Port)
                    {
                        EnableSsl = false
                    };
                }

                smtp?.Send(m);
            }
        }

        public void SendEmailWithFiles(string subject, 
                                       string text, 
                                       string[] filenames)
        {
            var from = new MailAddress(Settings.Instance.FromAddress);
            var toMail = new MailAddress(Settings.Instance.ToAddress);
            var m = new MailMessage(from, toMail);

            foreach (var path in filenames)
            {
                if (File.Exists(path))
                {
                    m.Attachments.Add(new Attachment(path));
                }
            }

            m.Subject = subject;
            m.Body = text;
            m.IsBodyHtml = true;
            SmtpClient smtp = null;

            if (Settings.Instance.NeedAuthorization)
            {
                smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                                      Settings.Instance.Port)
                {
                    Credentials = new NetworkCredential(Settings.Instance.Username,
                                                        Settings.Instance.Password),
                    EnableSsl = true
                };
            }
            else
            {
                smtp = new SmtpClient(Settings.Instance.SmtpAddress,
                                      Settings.Instance.Port)
                {
                    EnableSsl = false
                };
            }

            smtp?.Send(m);
        }
    }
}