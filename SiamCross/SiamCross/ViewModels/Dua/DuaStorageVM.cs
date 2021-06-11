using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dua;

namespace SiamCross.ViewModels.Dua
{
    public class DuaStorageVM : BaseStorageVM
    {
        private readonly DuaStorage _StorageModel;

        public ISensor Sensor { get; }
        public uint AviableRep => _StorageModel.AviableRep;
        public uint AviableEcho => _StorageModel.AviableEcho;


        public uint StartRep
        {
            get => _StorageModel.StartRep;
            set => _StorageModel.StartRep = value;
        }
        public uint CountRep
        {
            get => _StorageModel.CountRep;
            set => _StorageModel.CountRep = value;
        }
        public uint StartEcho
        {
            get => _StorageModel.StartEcho;
            set => _StorageModel.StartEcho = value;
        }
        public uint CountEcho
        {
            get => _StorageModel.CountEcho;
            set => _StorageModel.CountEcho = value;

        }




        public DuaStorageVM(ISensor sensor)
            : base(sensor.Model.Storage)
        {
            Sensor = sensor;
            _StorageModel = Model as DuaStorage;

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
