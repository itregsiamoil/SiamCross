using System.Threading.Tasks;

namespace SiamCross.Models.Tools
{
    public interface IEmailSender
    {
        Task<bool> SendEmailWithFiles(string subject, string text, string[] filenames);
    }
}
