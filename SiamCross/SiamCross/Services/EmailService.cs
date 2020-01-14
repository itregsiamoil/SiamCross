using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Services
{
    public sealed class EmailService
    {
        private static readonly Lazy<EmailService> _instance =
            new Lazy<EmailService>(() => new EmailService());
        public static EmailService Instance { get => _instance.Value; }

        private IEmailSender _emailSender;
        private EmailService()
        {
            _emailSender = AppContainer.Container.Resolve<IEmailSender>();
        }

        public void SendEmailWithFile(string filename)
        {
            _emailSender.SendEmailWithFile(filename);
        }

        public void SendEmail(string to, string subject, string text)
        {
            _emailSender.SendEmail(to, subject, text);
        }
    }
}
