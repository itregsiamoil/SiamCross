using SiamCross.Models;

namespace SiamCross.ViewModels
{
    public class DeviceInfoVM : BasePageVM
    {
        readonly DeviceInfoModel _Model;
        public DeviceInfoVM(DeviceInfoModel model)
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
        public string Kind => DeviceIndex.GetName(_Model.Kind);
        public uint Number => _Model.Number;
        public string Name => _Model.Name;
        public string Protocol
        {
            get
            {
                if (ProtocolIndex.Instance.TryGetName(_Model.ProtocolId, out string name))
                    return name;
                return string.Empty;
            }
        }
        public string Phy
        {
            get
            {
                if (PhyIndex.Instance.TryGetName(_Model.PhyId, out string name))
                    return name;
                return string.Empty;
            }
        }

    }
}
