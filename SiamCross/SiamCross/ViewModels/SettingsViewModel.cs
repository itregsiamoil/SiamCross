using SiamCross.Models.Tools;

namespace SiamCross.ViewModels
{
    public class SettingsViewModel : BaseViewModel, IViewModel
    {
        private string fromAddress;
        private string toAddress;
        private string smtpAddress;
        private string port;
        private string username;
        private string password;

        public string FromAddress
        {
            get => fromAddress;
            set
            {
                fromAddress = value;
                Settings.Instance.FromAddress = value;
            }
        }

        public string ToAddress
        {
            get => toAddress;
            set
            {
                toAddress = value;
                Settings.Instance.ToAddress = value;
            }
        }

        public string SmtpAddress
        {
            get => smtpAddress;
            set
            {
                smtpAddress = value;
                Settings.Instance.SmtpAddress = value;
            }
        }

        public string Port
        {
            get => port;
            set
            {
                port = value;
                if (int.TryParse(value, out int p))
                    Settings.Instance.Port = p;
            }
        }

        public string Username
        {
            get => username;
            set
            {
                username = value;
                Settings.Instance.Username = value;
            }
        }

        public string Password 
        { 
            get => password;
            set
            {
                password = value;
                Settings.Instance.Password = value;
            }
        }

        public SettingsViewModel()
        {
            FromAddress = Settings.Instance.FromAddress;
            ToAddress = Settings.Instance.ToAddress;
            SmtpAddress = Settings.Instance.SmtpAddress;
            Port = Settings.Instance.Port.ToString();
            Username = Settings.Instance.Username;
            Password = Settings.Instance.Password;
        }
    }
}
