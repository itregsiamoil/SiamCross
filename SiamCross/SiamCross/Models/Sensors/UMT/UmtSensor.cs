using SiamCross.Models.Connection;
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
        public UmtSensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
           : base(conn, deviceInfo)
        {
            Connection.AdditioonalTimeout = 2000;

            Position.TaskLoad = new TaskPositionLoad(Position);
            Position.TaskSave = new TaskPositionSave(Position);

            Connection.PropertyChanged += OnConnectionChange;

            SurveyCfg = new SurveyCfg(this);
            /*
            Storage = new DuaStorage(this);
            
            */
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.Static));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.Dynamic));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.PeriodicStatic));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.PeriodycDynamic));


        }
        async void OnConnectionChange(object sender, PropertyChangedEventArgs e)
        {
            if (null == sender || "State" != e.PropertyName)
                return;
            if (ConnectionState.Connected == Connection.State)
                await OnConnect();
            if (ConnectionState.Disconnected == Connection.State)
                OnDisconnect();
        }
        void OnDisconnect()
        {
            Position.ResetSaved();
            SurveyCfg.ResetSaved();
        }
        async Task OnConnect()
        {
            //await (Surveys[0].CmdWait as AsyncCommand)?.ExecuteAsync();
            await (Position.CmdLoad as AsyncCommand)?.ExecuteAsync();
        }
        public override void Dispose()
        {
            Connection.PropertyChanged -= OnConnectionChange;
            base.Dispose();
        }

    }

    public class UmtSensorVM : BaseSensor2
    {
        public UmtSensorVM(SensorModel model)
            : base(model)
        {

            foreach (var surveyModel in Model.Surveys)
            {
                var vm = new SurveyVM(this, surveyModel as UmtSurvey);
                SurveysVM.SurveysCollection.Add(vm);
            }

        }

        public override async Task<bool> QuickReport(CancellationToken cancelToken)
        {

            return false;
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
