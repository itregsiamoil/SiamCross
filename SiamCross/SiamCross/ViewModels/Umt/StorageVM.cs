using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Umt;

namespace SiamCross.ViewModels.Umt
{
    class StorageVM : BaseStorageVM
    {
        private readonly Storage _StorageModel;
        public ISensor Sensor { get; }

        public string EmptySpaceRatio => (_StorageModel.EmptySpaceRatio).ToString();
        public string TotalSpace => (_StorageModel.TotalSpace).ToString();
        public string SurveyQty => (_StorageModel.SurveyQty).ToString();

        public string TotalSpaceM => (_StorageModel.TotalSpace / 1024 / 1024).ToString();
        public string TotalSpaceK => (_StorageModel.TotalSpace % 1024).ToString();
        public string TotalSpaceB => (_StorageModel.TotalSpace % (1024)).ToString();

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
            if (nameof(TotalSpace) == e.PropertyName)
            {
                ChangeNotify(nameof(TotalSpaceM));
                ChangeNotify(nameof(TotalSpaceK));
                ChangeNotify(nameof(TotalSpaceB));
            }

            ChangeNotify(e.PropertyName);


        }
    }

}
