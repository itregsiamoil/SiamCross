using SiamCross.Models;
using System;

namespace SiamCross.ViewModels
{
    public class SurveyInfoVM : BasePageVM
    {
        readonly SurveyInfoModel _Model;
        public SurveyInfoVM(SurveyInfoModel model)
        {
            _Model = model;
            _Model.PropertyChanged += StorageModel_PropertyChanged;
        }
        void StorageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != _Model)
                return;
            ChangeNotify(e.PropertyName);
        }
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            _Model.PropertyChanged -= StorageModel_PropertyChanged;
        }
        public uint Kind
        {
            get => _Model.Kind;
            set => _Model.Kind = value;
        }
        public DateTime BeginTimestamp
        {
            get => _Model.BeginTimestamp;
            set => _Model.BeginTimestamp = value;
        }
        public DateTime EndTimestamp
        {
            get => _Model.EndTimestamp;
            set => _Model.EndTimestamp = value;
        }
        public string Comment
        {
            get => _Model.Comment;
            set => _Model.Comment = value;
        }
    }

}
