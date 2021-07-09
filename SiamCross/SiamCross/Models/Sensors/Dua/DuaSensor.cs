using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Dua.Surveys;
using SiamCross.ViewModels.Dua;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaSensorModel : SensorModel
    {
        private DuaSurveyCfg SurveyCfg;
        public DuaSensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
           : base(conn, deviceInfo)
        {
            Connection.AdditioonalTimeout = 2000;

            Position.TaskLoad = new TaskPositionLoad(Position);
            Position.TaskSave = new TaskPositionSave(Position);

            Storage = new DuaStorage(this);
            SurveyCfg = new DuaSurveyCfg(this);
            TaskWait = new TaskSurveyWait(this);

            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LStatic));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LDynamic));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LRC));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LDC));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.PAR));

            OnConnectQueue.Add(Position.TaskLoad);

            ConnHolder.CmdUpdateStatus = new AsyncCommand(
                UpdateStatus,
                () => Manager.IsFree,
                null, false, false);
        }
        async Task UpdateStatus()
        {
            var task = new TaskUpdateStatus(this);
            await Manager.Execute(task);
        }
    }


    public class DuaSensor : BaseSensor2
    {
        public DuaSensor(SensorModel model)
            : base(model)
        {
            StorageVM = new DuaStorageVM(this);

            //FactoryConfigVM = new FactoryConfigVM(this);
            //UserConfigVM = new UserConfigVM(this);
            //StateVM = new StateVM(this);


            Model.ConnHolder.PropertyChanged += OnHolderChange;
            IsNewStatus = true;
        }
        void OnHolderChange(object sender, PropertyChangedEventArgs e)
        {
            if (null == sender || nameof(Model.ConnHolder.IsActivated) != e.PropertyName)
                return;
            ChangeNotify(nameof(Activate));
        }
        public override void Dispose()
        {
            Model.ConnHolder.PropertyChanged -= OnHolderChange;
            base.Dispose();
        }

        public override Task<bool> QuickReport(CancellationToken cancelToken)
        {
            return Task.FromResult(true);
        }


        public override bool Activate
        {
            get => Model.ConnHolder.IsActivated;
            set
            {
                Model.ConnHolder.IsActivated = value;
                ChangeNotify(nameof(Activate));
            }
        }

    }//DuaSensor
}
