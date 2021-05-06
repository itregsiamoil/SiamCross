using System;

namespace SiamCross.Models
{
    public class DistributionInfoModel : ViewModels.BaseVM
    {
        readonly DistributionInfo _Data;
        public DistributionInfoModel(DistributionInfo data)
        {
            _Data = data;
        }
        public DateTime Timestamp
        {
            get => _Data.Timestamp;
            set => SetProperty(ref _Data.Timestamp, value);
        }
        public string Destination
        {
            get => _Data.Destination;
            set => SetProperty(ref _Data.Destination, value);
        }
    }
}
