using Android.Runtime;
using Newtonsoft.Json;
using SiamCross.Droid.Services;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(SettingsSaverAndroid))]
namespace SiamCross.Droid.Services
{
    [Preserve(AllMembers = true)]
    [Fody.ConfigureAwait(false)]
    public class SettingsSaverAndroid //: ISettingsSaver
    {
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);
        private const string _name = "settings.json";
        private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), _name);
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public SettingsSaverAndroid()
        {

        }
        public async Task<MailSettingsData> ReadSettings()
        {
            try
            {
                MailSettings result = null;
                using (await _mutex.UseWaitAsync())
                {
                    using StreamReader file = new StreamReader(_path);
                    while (!file.EndOfStream)
                    {
                        string line = await file.ReadLineAsync();
                        object item = JsonConvert.DeserializeObject(line, _jsonSettings);
                        if (item is MailSettings settings)
                            result = settings;
                    }
                    file.Close();
                }
                return result.GetData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION {ex.Message}\n{ex.StackTrace}");
            }
            return new MailSettingsData();
        }
        public async Task SaveSettings(MailSettingsData data)
        {
            try
            {
                using (await _mutex.UseWaitAsync())
                {
                    MailSettings settings = new MailSettings();
                    settings.SetData(data);
                    using (StreamWriter file = new StreamWriter(_path))
                    {
                        string jsonString = JsonConvert.SerializeObject(settings, _jsonSettings);
                        await file.WriteLineAsync(jsonString);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}