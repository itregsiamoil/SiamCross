using SiamCross.Models.Tools;

namespace SiamCross.ViewModels
{
    public class SettingsViewModel : BaseViewModel, IViewModel
    {
        private string _fromAddress;
        private string _toAddress;
        private string _smtpAddress;
        private string _port;
        private string _username;
        private string _password;
        private bool _needAuthorization;

        public bool NeedAuthorization
        {
            get => _needAuthorization;
            set
            {
                _needAuthorization = value;
                Settings.Instance.NeedAuthorization = value;
            }
        }
        public string FromAddress
        {
            get => _fromAddress;
            set
            {
                _fromAddress = value;
                Settings.Instance.FromAddress = value;
            }
        }

        public string ToAddress
        {
            get => _toAddress;
            set
            {
                _toAddress = value;
                Settings.Instance.ToAddress = value;
            }
        }

        public string SmtpAddress
        {
            get => _smtpAddress;
            set
            {
                _smtpAddress = value;
                Settings.Instance.SmtpAddress = value;
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                if (int.TryParse(value, out int p))
                    Settings.Instance.Port = p;
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                Settings.Instance.Username = value;
            }
        }

        public string Password 
        { 
            get => _password;
            set
            {
                _password = value;
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
