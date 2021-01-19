using Android.Runtime;
using Newtonsoft.Json;
using SiamCross.Droid.Services;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(SettingsSaverAndroid))]
namespace SiamCross.Droid.Services
{
    [Preserve(AllMembers = true)]
    public class SettingsSaverAndroid : ISettingsSaver
    {
        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1);

        private const string _name = "settings.json";

        private readonly string _path;

        public SettingsSaverAndroid()
        {
            _path = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), _name);
        }

        /// <summary>
        /// Прочитать настройки из json-файла.
        /// </summary>
        /// <returns>Настройки или null</returns>
        public async Task<SettingsParameters> ReadSettings()
        {
            SettingsParameters result = null;

            if (!File.Exists(_path)) return null;

            await _mutex.WaitAsync();
            try
            {
                StreamReader file = new StreamReader(_path);

                while (!file.EndOfStream)
                {
                    string line = await file.ReadLineAsync();

                    object item = JsonConvert.DeserializeObject(
                        line, _jsonSettings);

                    if (item is SettingsParameters settings)
                    {
                        result = settings;
                    }
                }

                file.Dispose();
            }
            finally
            {
                _mutex.Release();
            }


            return result;
        }

        public async Task SaveSettings(SettingsParameters settings)
        {
            await _mutex.WaitAsync();

            try
            {
                using (StreamWriter file = new StreamWriter(_path))
                {
                    string jsonString = JsonConvert.SerializeObject(settings,
                                                            _jsonSettings);
                    await file.WriteLineAsync(jsonString);
                }
            }
            finally
            {
                _mutex.Release();
            }
        }

        protected static readonly JsonSerializerSettings
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
    }
}