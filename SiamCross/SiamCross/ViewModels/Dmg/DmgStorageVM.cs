using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg;

namespace SiamCross.ViewModels.Dmg
{
    public class DmgStorageVM : BaseStorageVM
    {
        private readonly DmgStorage _StorageModel;
        public ISensor Sensor { get; }
        public int Aviable => _StorageModel.Aviable();
        public bool OpenOnDownload
        {
            get => _StorageModel.OpenOnDownload;
            set => SetProperty(ref _StorageModel.OpenOnDownload, value);
        }
        public DmgStorageVM(ISensor sensor)
            : base(sensor.Model.Storage)
        {
            Sensor = sensor;
            _StorageModel = Model as DmgStorage;

            CmdUpdateStorageInfo = Model.CmdUpdateStorageInfo;
            CmdDownload = Model.CmdDownload;
            CmdClearStorage = Model.CmdClearStorage;
        }
    }
}
