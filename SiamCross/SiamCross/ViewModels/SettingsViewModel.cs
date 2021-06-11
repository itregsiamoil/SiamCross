using SiamCross.Models.Tools;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SettingsViewModel : BasePageVM
    {
        private MailSettingsData _MailSettingsData = new MailSettingsData();
        public ICommand CmdDefault { get; }
        public ICommand CmdSave { get; }

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
        public SettingsViewModel()
        {
            CmdDefault = new AsyncCommand(DoDefault
               , (Func<object, bool>)null, null, false, false);
            CmdSave = new AsyncCommand(DoSave
                , (Func<object, bool>)null, null, false, false);

            _MailSettingsData = Settings.Instance.GetData();
            /*
            ChangeNotify(nameof(FromName));
            ChangeNotify(nameof(SubjectName));
            ChangeNotify(nameof(FromAddress));
            ChangeNotify(nameof(ToAddress));
            ChangeNotify(nameof(SmtpAddress));
            ChangeNotify(nameof(Port));
            ChangeNotify(nameof(Username));
            ChangeNotify(nameof(Password));
            ChangeNotify(nameof(NeedAuthorization));
            */
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }

        private Task DoDefault()
        {
            FromName = "siam";
            SubjectName = "Siam Measurements";
            FromAddress = "sudos@kb.siamoil.ru";
            ToAddress = string.Empty;
            SmtpAddress = "kb.siamoil.ru";
            Port = 25;
            Username = "sudos";
            Password = "zEsPe5";
            NeedAuthorization = true;
            return Task.CompletedTask;
        }
        private async Task DoSave()
        {
            Settings.Instance.SetData(_MailSettingsData);
            await Settings.Instance.SaveSettings();
        }
    }
}
