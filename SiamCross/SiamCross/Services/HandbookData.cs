using Newtonsoft.Json;
using SiamCross.AppObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public class HandbookData : IDisposable
    {
        private static readonly Lazy<HandbookData> _instance =
            new Lazy<HandbookData>(() => new HandbookData());

        public static HandbookData Instance { get => _instance.Value; }
        public IHandbookManager _handbookManager;

        private Dictionary<string, int> _fieldDictionary;

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
        }

        public Dictionary<string, int> GetFieldDictionary()
        {
            if (_fieldDictionary == null)
            {
                _fieldDictionary = _handbookManager.LoadFields();
            }

            return _fieldDictionary;
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
    }
}
