using Xamarin.Forms.Internals;

namespace SiamCross.Models.Tools
{
    public struct MailSettingsData
    {
        public string FromName;
        public string SubjectName;
        public string FromAddress;
        public string ToAddress;
        public string SmtpAddress;
        public int Port;
        public string Username;
        public string Password;
        public bool NeedAuthorization;
    }

    [Preserve(AllMembers = true)]
    public class MailSettings : ViewModels.BaseVM
    {
        private MailSettingsData _MailSettingsData = new MailSettingsData();
        public string FromName
        {
            get => _MailSettingsData.FromName;
            set => SetProperty(ref _MailSettingsData.FromName, value);
        }
        public string SubjectName
        {
            get => _MailSettingsData.SubjectName;
            set => SetProperty(ref _MailSettingsData.SubjectName, value);
        }
        public bool NeedAuthorization
        {
            get => _MailSettingsData.NeedAuthorization;
            set => SetProperty(ref _MailSettingsData.NeedAuthorization, value);
        }
        public string FromAddress
        {
            get => _MailSettingsData.FromAddress;
            set => SetProperty(ref _MailSettingsData.FromAddress, value);
        }
        public string ToAddress
        {
            get => _MailSettingsData.ToAddress;
            set => SetProperty(ref _MailSettingsData.ToAddress, value);
        }
        public string SmtpAddress
        {
            get => _MailSettingsData.SmtpAddress;
            set => SetProperty(ref _MailSettingsData.SmtpAddress, value);
        }
        public int Port
        {
            get => _MailSettingsData.Port;
            set => SetProperty(ref _MailSettingsData.Port, value);
        }
        public string Username
        {
            get => _MailSettingsData.Username;
            set => SetProperty(ref _MailSettingsData.Username, value);
        }
        public string Password
        {
            get => _MailSettingsData.Password;
            set => SetProperty(ref _MailSettingsData.Password, value);
        }
        public void SetData(MailSettingsData data)
        {
            _MailSettingsData = data;
        }
        public MailSettingsData GetData()
        {
            return _MailSettingsData;
        }
        public MailSettings(MailSettingsData data = new MailSettingsData())
        {
            _MailSettingsData = data;
        }
    }
}
