namespace SiamCross.Models.Tools
{
    public class SettingsParameters
    {
        public string FromName { get; set; }
        public string SubjectName { get; set; }
        public string FromAddress { get; }
        public string ToAddress { get; }
        public string SmtpAddress { get; }
        public int Port { get; }
        public string Username { get; }
        public string Password { get; }
        public bool NeedAuthorization { get; }

        public SettingsParameters(
            string fromName, string subjectName,
            string fromAddress, string toAddress,
            string smtpAddress, int port,
            string username, string password,
            bool needAuthorization)
        {
            FromName = fromName;
            SubjectName = subjectName;
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
