﻿using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;

namespace SiamCross.Services
{
    public class HandbookData : IDisposable
    {
        private static readonly Lazy<HandbookData> _instance =
            new Lazy<HandbookData>(() => new HandbookData());

        public static HandbookData Instance => _instance.Value;
        private readonly IHandbookManager _handbookManager;

        private Dictionary<string, int> _fieldDictionary;
        private List<SoundSpeedModel> _soundSpeedList;

        private HandbookData()
        {
            _handbookManager = AppContainer.Container.Resolve<IHandbookManager>();
        }

        public void Dispose()
        {
            if (_fieldDictionary != null)
            {
                _handbookManager.SaveFields(_fieldDictionary);
            }

            if (_soundSpeedList != null)
            {
                _handbookManager.SaveSoundSpeeds(_soundSpeedList);
            }
        }

        #region Fields
        public Dictionary<string, int> GetFieldDictionary()
        {
            if (_fieldDictionary == null)
            {
                _fieldDictionary = _handbookManager.LoadFields();
            }

            return _fieldDictionary;
        }

        public IEnumerable<string> GetFieldList()
        {
            Dictionary<string, int> fieldDictionay = GetFieldDictionary();
            List<string> fieldList = new List<string>();
            foreach (KeyValuePair<string, int> field in fieldDictionay)
            {
                fieldList.Add(field.Key + ": " + field.Value);
            }

            return fieldList;
        }

        public void AddField(string fieldKey, int fieldValue)
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

                _handbookManager.SaveFields(_fieldDictionary);
            }
        }

        public void RemoveField(string key)
        {
            if (_fieldDictionary != null)
            {
                if (_fieldDictionary.ContainsKey(key))
                {
                    _fieldDictionary.Remove(key);
                }
            }

            _handbookManager.SaveFields(_fieldDictionary);
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
