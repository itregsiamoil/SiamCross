using Xamarin.Forms.Internals;

namespace SiamCross.Models.Tools
{
    [Preserve(AllMembers = true)]
    public class SettingsParameters
    {
        public string FromAddress { get; }
        public string ToAddress { get;  }
        public string SmtpAddress { get;  }
        public int Port { get; }
        public string Username { get; }
        public string Password { get; }

        public SettingsParameters(string fromAddress, string toAddress, 
            string smtpAddress, int port, 
            string username, string password)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            SmtpAddress = smtpAddress;
            Port = port;
            Username = username;
            Password = password;
        }
    }
}
