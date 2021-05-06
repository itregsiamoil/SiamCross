using System.Collections.Generic;

namespace SiamCross.Models
{
    public class DeviceInfoModel : ViewModels.BaseVM
    {
        readonly DeviceInfo _Data;
        public DeviceInfoModel(DeviceInfo data)
        {
            _Data = data;
        }
        public uint Kind
        {
            get => _Data.Kind;
            set => SetProperty(ref _Data.Kind, value);
        }
        public uint Number
        {
            get => _Data.Number;
            set => SetProperty(ref _Data.Number, value);
        }
        public string Name
        {
            get => _Data.Name;
            set => SetProperty(ref _Data.Name, value);
        }
        public uint ProtocolId
        {
            get => _Data.ProtocolId;
            set => SetProperty(ref _Data.ProtocolId, value);
        }
        public uint PhyId
        {
            get => _Data.PhyId;
            set => SetProperty(ref _Data.PhyId, value);
        }
        public Dictionary<string, object> PhyData
        {
            get => _Data.PhyData;
            set => SetProperty(ref _Data.PhyData, value);
        }
        public Dictionary<string, object> DeviceData
        {
            get => _Data.DeviceData;
            set => SetProperty(ref _Data.DeviceData, value);
        }
    }
}
