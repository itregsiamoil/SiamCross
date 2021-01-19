using Android.Runtime;
using Newtonsoft.Json;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SiamCross.Droid.Models
{
    [Preserve(AllMembers = true)]
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
            Dictionary<string, int> fieldDictionary = new Dictionary<string, int>();

            lock (_fieldFileLocker)
            {
                string backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), "Fields.json");

                if (backingFile == null || !File.Exists(backingFile))
                {
                    return fieldDictionary;
                }

                StreamReader file = new StreamReader(backingFile, true);

                if (file != null)
                {
                    while (!file.EndOfStream)
                    {
                        string line = file.ReadLine();

                        object item = JsonConvert.DeserializeObject(
                            line, _settings);

                        switch (item)
                        {
                            case Dictionary<string, int> fieldPair:
                                foreach (KeyValuePair<string, int> pair in fieldPair)
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

                string backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal),
                    "Fields.json");

                using (StreamWriter file = File.CreateText(backingFile))
                {
                    foreach (KeyValuePair<string, int> field in fieldDict)
                    {
                        string jsonString = JsonConvert.SerializeObject(
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
            List<SoundSpeedModel> soundSpeedList = new List<SoundSpeedModel>();

            lock (_soundSpeedFileLocker)
            {
                string backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), "SoundSpeed.json");

                if (backingFile == null || !File.Exists(backingFile))
                {
                    soundSpeedList.AddRange(CreateDefaultSoundSpeeds());
                    SaveSoundSpeeds(soundSpeedList);

                    return soundSpeedList;
                }

                StreamReader file = new StreamReader(backingFile, true);

                if (file != null)
                {
                    while (!file.EndOfStream)
                    {
                        string line = file.ReadLine();

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

                string backingFile = Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal),
                    "SoundSpeed.json");

                using (StreamWriter file = File.CreateText(backingFile))
                {
                    foreach (SoundSpeedModel speed in soundSpeedList)
                    {
                        string jsonString = JsonConvert.SerializeObject(
                            speed, _settings);

                        file.WriteLine(jsonString);
                    }
                }
            }
        }

        public List<SoundSpeedModel> CreateDefaultSoundSpeeds()
        {
            Assembly assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            SoundSpeedFileParcer soundFileParcer = new SoundSpeedFileParcer();

            List<KeyValuePair<float, float>> langepasDictionary;
            List<KeyValuePair<float, float>> tatariaDictonary;

            Stream langepasStream = assembly.GetManifestResourceStream("SiamCross.DefaultSoundSpeedResources.langepas");
            using (StreamReader reader = new StreamReader(langepasStream))
            {
                string text = reader.ReadToEnd();
                langepasDictionary = soundFileParcer.TryToParce(text);
            }

            Stream tatariaStream = assembly.GetManifestResourceStream("SiamCross.DefaultSoundSpeedResources.tataria");
            using (StreamReader reader = new StreamReader(tatariaStream))
            {
                string text = reader.ReadToEnd();
                tatariaDictonary = soundFileParcer.TryToParce(text);
            }

            return new List<SoundSpeedModel>()
            {
                new SoundSpeedModel(1, SiamCross.Resource.Langepas, langepasDictionary),
                new SoundSpeedModel(2, SiamCross.Resource.Tataria, tatariaDictonary)
            };
        }
        #endregion
    }
}