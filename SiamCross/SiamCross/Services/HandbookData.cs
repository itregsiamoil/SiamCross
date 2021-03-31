using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class HandbookData : IDisposable
    {
        private static readonly Lazy<HandbookData> _instance =
            new Lazy<HandbookData>(() => new HandbookData());

        public static HandbookData Instance => _instance.Value;
        private readonly IHandbookManager _handbookManager;

        readonly private Dictionary<string, long> _fieldDictionary = new Dictionary<string, long>();
        private List<SoundSpeedModel> _soundSpeedList;

        private HandbookData()
        {
            _handbookManager = AppContainer.Container.Resolve<IHandbookManager>();
        }
        public async Task Init()
        {
            var values = await DataRepository.Instance.FieldDictionary.Load();
            foreach (var v in values)
                _fieldDictionary.Add(v.Title, v.Id);
        }

        public void Dispose()
        {
            if (_fieldDictionary != null)
            {
                //await _handbookManager.SaveFieldsAsync(_fieldDictionary);
            }

            if (_soundSpeedList != null)
            {
                _handbookManager.SaveSoundSpeeds(_soundSpeedList);
            }
        }

        #region Fields
        public Dictionary<string, long> GetFieldDictionary()
        {
            return _fieldDictionary;
        }

        public IEnumerable<string> GetFieldList()
        {
            List<string> fieldList = new List<string>();
            foreach (KeyValuePair<string, long> field in _fieldDictionary)
            {
                fieldList.Add(field.Key + ": " + field.Value);
            }
            return fieldList;
        }

        public async void AddField(string fieldKey, long fieldValue)
        {
            if (_fieldDictionary != null)
            {
                if (!_fieldDictionary.ContainsKey(fieldKey))
                {
                    _fieldDictionary.Add(fieldKey, fieldValue);
                }
                else
                {
                    _fieldDictionary.Remove(fieldKey);
                    _fieldDictionary.Add(fieldKey, fieldValue);
                }
                await DataRepository.Instance.FieldDictionary.Save(fieldKey, fieldValue);
            }
        }

        public async void RemoveField(string key)
        {
            if (_fieldDictionary != null)
            {
                if (_fieldDictionary.ContainsKey(key))
                {
                    if (_fieldDictionary.TryGetValue(key, out long val))
                    {
                        await DataRepository.Instance.FieldDictionary.Delete(val);
                        _fieldDictionary.Remove(key);
                    }
                }
            }
        }

        #endregion

        #region SoundSpeed

        public List<SoundSpeedModel> GetSoundSpeedList()
        {
            if (_soundSpeedList == null)
            {
                _soundSpeedList = _handbookManager.LoadSoundSpeeds();
            }

            return _soundSpeedList;
        }

        public void AddSoundSpeed(SoundSpeedModel soundSpeed)
        {
            if (_soundSpeedList != null)
            {
                _soundSpeedList.Add(soundSpeed);
                _handbookManager.SaveSoundSpeeds(_soundSpeedList);
            }
        }

        public void RemoveSoundSpeed(SoundSpeedModel soundSpeed)
        {
            if (_soundSpeedList != null)
            {
                _soundSpeedList.Remove(soundSpeed);
            }

            _handbookManager.SaveSoundSpeeds(_soundSpeedList);
        }

        #endregion
    }
}
