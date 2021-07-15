using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg;

namespace SiamCross.ViewModels.Dmg
{
    public class StorageVM : BaseStorageVM
    {
        private readonly DmgStorage _StorageModel;
        public ISensor Sensor { get; }
        public int Aviable => _StorageModel.Aviable();
        public bool OpenOnDownload
        {
            get => _StorageModel.OpenOnDownload;
            set => SetProperty(ref _StorageModel.OpenOnDownload, value);
        }
        public StorageVM(ISensor sensor)
            : base(sensor.Model.Storage)
        {
            Sensor = sensor;
            _StorageModel = Model as DmgStorage;

            _StorageModel.PropertyChanged += StorageModel_PropertyChanged;
            Sensor.Model.Manager.OnChangeTask.ProgressChanged += SetTask;
        }
        public override void Unsubscribe()
        {
            base.Unsubscribe();
            _StorageModel.PropertyChanged -= StorageModel_PropertyChanged;
            Sensor.Model.Manager.OnChangeTask.ProgressChanged -= SetTask;
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        void SetTask(object obj, ITask task)
        {
            RaiseCanExecuteChanged(CmdUpdateStorageInfo);
            RaiseCanExecuteChanged(CmdDownload);
            RaiseCanExecuteChanged(CmdClearStorage);
        }
        private void StorageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != Model)
                return;
            /*
            if (nameof(TotalSpace) == e.PropertyName)
            {
                ChangeNotify(nameof(TotalSpaceM));
                ChangeNotify(nameof(TotalSpaceK));
                ChangeNotify(nameof(TotalSpaceB));
            }
            */
            ChangeNotify(e.PropertyName);


        }
    }
}
