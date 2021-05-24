using System;
using System.Collections.Generic;

namespace SiamCross.Models
{
    public class SurveyInfoModel : ViewModels.BaseVM
    {
        readonly MeasurementInfo _Data;
        public SurveyInfoModel(MeasurementInfo data)
        {
            _Data = data;
        }
        public uint Kind
        {
            get => _Data.Kind;
            set => SetProperty(ref _Data.Kind, value);
        }
        public DateTime BeginTimestamp
        {
            get => _Data.BeginTimestamp;
            set => SetProperty(ref _Data.BeginTimestamp, value);
        }
        public DateTime EndTimestamp
        {
            get => _Data.EndTimestamp;
            set => SetProperty(ref _Data.EndTimestamp, value);
        }
        public string Comment
        {
            get => _Data.Comment;
            set => SetProperty(ref _Data.Comment, value);
        }
        public Dictionary<AttributeItem, long> DataInt
        {
            get => _Data.DataInt;
            set => SetProperty(ref _Data.DataInt, value);
        }
        public Dictionary<AttributeItem, double> DataFloat
        {
            get => _Data.DataFloat;
            set => SetProperty(ref _Data.DataFloat, value);
        }
        public Dictionary<AttributeItem, string> DataString
        {
            get => _Data.DataString;
            set => SetProperty(ref _Data.DataString, value);
        }
        public Dictionary<AttributeItem, string> DataBlob
        {
            get => _Data.DataBlob;
            set => SetProperty(ref _Data.DataBlob, value);
        }
        public bool TryGet(string title, out long value)
        {
            return _Data.TryGet(title, out value);
        }
        public bool TryGet(string title, out double value)
        {
            return _Data.TryGet(title, out value);
        }
        public bool TryGet(string title, out string value)
        {
            return _Data.TryGet(title, out value);
        }
        public bool TryGetBlob(string title, out string value)
        {
            return _Data.TryGetBlob(title, out value);
        }
    }
}
