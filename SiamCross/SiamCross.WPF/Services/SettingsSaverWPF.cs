﻿using Newtonsoft.Json;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.WPF.Services;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(SettingsSaverWPF))]
namespace SiamCross.WPF.Services
{
    public class SettingsSaverWPF : ISettingsSaver
    {
        private const string _name = "settings.json";

        private readonly string _path;

        public SettingsSaverWPF()
        {
            _path = Path.Combine(Directory.GetCurrentDirectory(), _name);
        }

        public bool DoesSettingsFileExists() => File.Exists(_path);

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
            if (!DoesSettingsFileExists()) return;

            using (var file = new StreamWriter(_path))
            {
                var jsonString = JsonConvert.SerializeObject(settings,
                                        _jsonSettings);
                file.WriteLine(jsonString);
            }
        }

        private static readonly JsonSerializerSettings
            _jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
    }
}
