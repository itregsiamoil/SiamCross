using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg;

namespace SiamCross.ViewModels.Dmg
{
    public class StorageVM : BaseStorageVM
    {
        private readonly Storage _StorageModel;
        public ISensor Sensor { get; }
        public bool Aviable => _StorageModel.Aviable;

        public StorageVM(ISensor sensor)
            : base(sensor.Model.Storage)
        {
            Sensor = sensor;
            _StorageModel = Model as Storage;

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

            ChangeNotify(e.PropertyName);
        }
    }
}
