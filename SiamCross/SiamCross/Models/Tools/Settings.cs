using Autofac;
using SiamCross.AppObjects;
using SiamCross.Services;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models.Tools
{
    public sealed class Settings : IDisposable
    {
        private static readonly Lazy<Settings> _instance =
            new Lazy<Settings>(() => new Settings());

        public static Settings Instance => _instance.Value;

        private readonly ISettingsSaver _settingsSaver;

        private Settings()
        {
            _settingsSaver = AppContainer.Container.Resolve<ISettingsSaver>();
        }

        public async Task Initialize()
        {
            SettingsParameters settings = await _settingsSaver.ReadSettings();
            if (settings != null) //Если файл настроек не пустой
            {
                FromName = settings.FromName;
                SubjectName = settings.SubjectName;
                FromAddress = settings.FromAddress;
                ToAddress = settings.ToAddress;
                SmtpAddress = settings.SmtpAddress;
                Port = settings.Port;
                Username = settings.Username;
                Password = settings.Password;
                IsNeedAuthorization = settings.NeedAuthorization;
            }
            else 
            {
                FromName = "sudos";
                SubjectName = "Siam Measurements";
                FromAddress = "ddin@kb.siamoil.ru";
                SmtpAddress = "mail.siamoil.ru";
                Port = 25;
                Username = string.Empty;
                Password = string.Empty;
                IsNeedAuthorization = false;
            }
        }

        public void Dispose()
        {
            _settingsSaver.SaveSettings(new SettingsParameters(
                FromName,
                SubjectName,
                FromAddress,
                ToAddress,
                SmtpAddress,
                Port,
                Username,
                Password,
                IsNeedAuthorization));
        }

        public async Task SaveSettings()
        {
            await _settingsSaver.SaveSettings(new SettingsParameters(
                FromName,
                SubjectName,
                 FromAddress,
                 ToAddress,
                 SmtpAddress,
                 Port,
                 Username,
                 Password,
                 IsNeedAuthorization));
        }

        public string FromName { get; set; }
        public string SubjectName { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string SmtpAddress { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsNeedAuthorization { get; set; }
    }
}
