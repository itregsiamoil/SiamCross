using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Services.Email
{
    public sealed class EmailService
    {
        private static readonly Lazy<EmailService> _instance =
            new Lazy<EmailService>(() => new EmailService());
        public static EmailService Instance => _instance.Value;

        private readonly EmailSender _emailSender;
        private EmailService()
        {
            _emailSender = new EmailSender();
        }
        public async Task<bool> SendEmailWithFilesAsync(string subject
            , string text, IReadOnlyCollection<string> filenames)
        {
            return await _emailSender.SendEmailWithFilesAsync(subject, text, filenames);
        }
    }
}
