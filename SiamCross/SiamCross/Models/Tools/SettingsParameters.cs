namespace SiamCross.Models.Tools
{
    public class SettingsParameters
    {
        public string FromAddress { get; }
        public string ToAddress { get;  }
        public string SmtpAddress { get;  }
        public int Port { get; }
        public string Username { get; }
        public string Password { get; }
        public bool NeedAuthorization { get; }

        public SettingsParameters(string fromAddress, string toAddress, 
            string smtpAddress, int port, 
            string username, string password, 
            bool needAuthorization)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            SmtpAddress = smtpAddress;
            Port = port;
            Username = username;
            Password = password;
            NeedAuthorization = needAuthorization;
        }
    }
}
