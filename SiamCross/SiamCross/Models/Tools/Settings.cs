using Autofac;
using SiamCross.AppObjects;
using SiamCross.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Tools
{
    [Preserve(AllMembers = true)]
    public sealed class Settings : IDisposable
    {
        private static readonly Lazy<Settings> _instance =
            new Lazy<Settings>(() => new Settings());

        public static Settings Instance { get => _instance.Value; }

        private readonly ISettingsSaver _settingsSaver;

        private Settings()
        {
            _settingsSaver = AppContainer.Container.Resolve<ISettingsSaver>();
        }

        public async Task Initialize()
        {
            var settings = await _settingsSaver.ReadSettings();
            if (settings != null) //Если файл настроек не пустой
            {
                FromAddress = settings.FromAddress;
                ToAddress = settings.ToAddress;
                SmtpAddress = settings.SmtpAddress;
                Port = settings.Port;
                Username = settings.Username;
                Password = settings.Password;
            }
        }

        public void Dispose()
        {
            _settingsSaver.SaveSettings(new SettingsParameters(
                FromAddress,
                ToAddress,
                SmtpAddress,
                Port,
                Username,
                Password));
        }

        public async Task SaveSettings()
        {
           await  _settingsSaver.SaveSettings(new SettingsParameters(
                FromAddress,
                ToAddress,
                SmtpAddress,
                Port,
                Username,
                Password));
        }

        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string SmtpAddress { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool NeedAuthorization { get; set; }
    }
}
