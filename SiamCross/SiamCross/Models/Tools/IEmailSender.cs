namespace SiamCross.Models.Tools
{
    public interface IEmailSender
    {
        void SendEmailWithFile(string filename);

        void SendEmail(string to, string subject, string text);
    }
}
