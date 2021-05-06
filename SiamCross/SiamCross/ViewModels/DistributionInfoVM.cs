using SiamCross.Models;
using System;

namespace SiamCross.ViewModels
{
    public class DistributionInfoVM : BasePageVM
    {
        readonly DistributionInfoModel _Model;
        public DistributionInfoVM(DistributionInfoModel model)
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
            _Model.PropertyChanged -= StorageModel_PropertyChanged;
        }
        public DateTime Timestamp
        {
            get => _Model.Timestamp;
            set => _Model.Timestamp = value;
        }
        public string Destination
        {
            get => _Model.Destination;
            set => _Model.Destination = value;
        }

    }
}
