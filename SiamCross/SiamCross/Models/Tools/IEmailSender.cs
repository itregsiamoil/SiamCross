using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Tools
{
    public interface IEmailSender
    {
        Task SendEmailWithFile(string filename);

        Task SendEmail(string to, string subject, string text);
    }
}
