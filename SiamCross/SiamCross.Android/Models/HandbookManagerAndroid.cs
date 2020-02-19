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

namespace SiamCross.Droid.Models
{
    public class HandbookManagerAndroid : IHandbookManager
    {
        private static readonly JsonSerializerSettings
        _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private readonly object _locker = new object();
        public Dictionary<string, int> LoadFields()
        {
            var fieldDictionary = new Dictionary<string, int>();

            lock (_locker)
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
            lock (_locker)
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
    }
}