using Newtonsoft.Json;
using SiamCross.Droid.Services;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(SettingsSaverAndroid))]
namespace SiamCross.Droid.Services
{
    public class SettingsSaverAndroid : ISettingsSaver
    {
        private const string _name = "settings.json";

        private readonly string _path;

        public SettingsSaverAndroid()
        {
            _path = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), _name);
        }

        public bool DoesSettingsFileExists()
        {
            return File.Exists(_path);
        }

        public SettingsParameters ReadSettings()
        {
            SettingsParameters result = null;

            if (!DoesSettingsFileExists()) return result;

            var file = new StreamReader(_path);

            if (file == null) return result;

            while (!file.EndOfStream)
            {
                var line = file.ReadLine();

                object item = JsonConvert.DeserializeObject(
                    line, _jsonSettings);

                if (item is SettingsParameters settings)
                {
                    result = settings;
                }
            }

            return result;
        }

        public void SaveSettings(SettingsParameters settings)
        {
            using (var file = new StreamWriter(_path))
            {
                var jsonString = JsonConvert.SerializeObject(settings,
                                                        _jsonSettings);
                file.WriteLine(jsonString);
            }
        }

        protected static readonly JsonSerializerSettings
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
    }
}