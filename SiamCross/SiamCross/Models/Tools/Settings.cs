using Autofac;
using SiamCross.AppObjects;
using SiamCross.Services;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models.Tools
{
    public sealed class Settings : MailSettings, IDisposable
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
            SetData(await _settingsSaver.ReadSettings());
        }
        public async void Dispose()
        {
            await SaveSettings();
        }
        public async Task SaveSettings()
        {
            await _settingsSaver.SaveSettings(GetData());
        }


    }
}
