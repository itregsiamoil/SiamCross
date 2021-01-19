using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Tools;
using System;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public sealed class EmailService
    {
        private static readonly Lazy<EmailService> _instance =
            new Lazy<EmailService>(() => new EmailService());
        public static EmailService Instance => _instance.Value;

        private readonly IEmailSender _emailSender;
        private EmailService()
        {
            _emailSender = AppContainer.Container.Resolve<IEmailSender>();
        }
        public Task<bool> SendEmailWithFiles(string subject
            , string text, string[] filenames)
        {
            return _emailSender.SendEmailWithFiles(subject, text, filenames);
        }
    }
}
