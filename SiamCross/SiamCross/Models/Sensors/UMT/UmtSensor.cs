using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Umt.Surveys;
using SiamCross.ViewModels.Umt;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Umt
{
    public class UmtSensorModel : SensorModel
    {
        private SurveyCfg SurveyCfg;
        public UmtSensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
           : base(conn, deviceInfo)
        {
            Connection.AdditioonalTimeout = 4000;

            Position.TaskLoad = new TaskPositionLoad(Position);
            Position.TaskSave = new TaskPositionSave(Position);

            Storage = new Storage(this);
            SurveyCfg = new SurveyCfg(this);
            TaskWait = new TaskSurveyWait(this);

            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.Static));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.Dynamic));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.PeriodicStatic));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.PeriodycDynamic));

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

    public class UmtSensorVM : BaseSensor2
    {
        public UmtSensorVM(SensorModel model)
            : base(model)
        {
            StorageVM = new StorageVM(this);

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
