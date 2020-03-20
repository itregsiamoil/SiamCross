using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SiamCross.Services;
using SiamCross.Models.Tools;
using System.Reflection;

namespace SiamCross.Droid.Models
{
    public class HandbookManagerAndroid : IHandbookManager
    {
        private static readonly JsonSerializerSettings
        _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly object _fieldFileLocker = new object();
        private readonly object _soundSpeedFileLocker = new object();

        #region Field
        public Dictionary<string, int> LoadFields()
        {
            var fieldDictionary = new Dictionary<string, int>();

            lock (_fieldFileLocker)
            {
                var backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), "Fields.json");

                if (backingFile == null || !File.Exists(backingFile))
                {
                    return fieldDictionary;
                }

                var file = new StreamReader(backingFile, true);

                if (file != null)
                {
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine();

                        object item = JsonConvert.DeserializeObject(
                            line, _settings);

                        switch (item)
                        {
                            case Dictionary<string, int> fieldPair:
                                foreach (var pair in fieldPair)
                                {
                                    fieldDictionary.TryAdd(pair.Key, pair.Value);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                file.Close();
            }

            return fieldDictionary;
        }

        public void SaveFields(Dictionary<string, int> fieldDict)
        {
            lock (_fieldFileLocker)
            {

                var backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal),
                    "Fields.json");

                using (var file = File.CreateText(backingFile))
                {
                    foreach (var field in fieldDict)
                    {
                        var jsonString = JsonConvert.SerializeObject(
                            fieldDict, _settings);

                        file.WriteLine(jsonString);
                    }
                }
            }
        }

        #endregion

        #region SoundSpeed

        public List<SoundSpeedModel> LoadSoundSpeeds()
        {
            var soundSpeedList = new List<SoundSpeedModel>();

            lock (_soundSpeedFileLocker)
            {
                var backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), "SoundSpeed.json");

                if (backingFile == null || !File.Exists(backingFile))
                {
                    soundSpeedList.AddRange(CreateDefaultSoundSpeeds());
                    SaveSoundSpeeds(soundSpeedList);

                    return soundSpeedList;
                }

                var file = new StreamReader(backingFile, true);

                if (file != null)
                {
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine();

                        object item = JsonConvert.DeserializeObject(
                            line, _settings);

                        switch (item)
                        {
                            case SoundSpeedModel soundSpeedItem:
                                soundSpeedList.Add(soundSpeedItem);
                                break;
                            default:
                                break;
                        }
                    }
                }
                file.Close();
            }
            return soundSpeedList;
        }

        public void SaveSoundSpeeds(List<SoundSpeedModel> soundSpeedList)
        {
            lock (_soundSpeedFileLocker)
            {

                var backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal),
                    "SoundSpeed.json");

                using (var file = File.CreateText(backingFile))
                {
                    foreach (var speed in soundSpeedList)
                    {
                        var jsonString = JsonConvert.SerializeObject(
                            speed, _settings);

                        file.WriteLine(jsonString);
                    }
                }
            }
        }

        public List<SoundSpeedModel> CreateDefaultSoundSpeeds()
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            var soundFileParcer = new SoundSpeedFileParcer();

            List<KeyValuePair<float, float>> langepasDictionary;
            List<KeyValuePair<float, float>> tatariaDictonary;

            Stream langepasStream = assembly.GetManifestResourceStream("SiamCross.DefaultSoundSpeedResources.langepas");
            using (var reader = new StreamReader(langepasStream))
            {
                var text = reader.ReadToEnd();
                langepasDictionary = soundFileParcer.TryToParce(text);
            }

            Stream tatariaStream = assembly.GetManifestResourceStream("SiamCross.DefaultSoundSpeedResources.tataria");
            using (var reader = new StreamReader(tatariaStream))
            {
                var text = reader.ReadToEnd();
                tatariaDictonary = soundFileParcer.TryToParce(text);
            }       

            return new List<SoundSpeedModel>()
            {
                new SoundSpeedModel(0, "Лангепас", langepasDictionary),
                new SoundSpeedModel(1, "Татария", tatariaDictonary)
            };
        }
        #endregion
    }
}