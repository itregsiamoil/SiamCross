using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.UMT
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

            /*
            Storage = new DuaStorage(this);
            SurveyCfg = new DuaSurveyCfg(this);

            Surveys.Add(new DuaSurvey(this, SurveyCfg
                , Kind.LStatic.Title()
                , Kind.LStatic.Info(), 1));
            Surveys.Add(new DuaSurvey(this, SurveyCfg
                , Kind.LDynamic.Title()
                , Kind.LDynamic.Info(), 2));
            Surveys.Add(new DuaSurvey(this, SurveyCfg
                , Kind.LRC.Title()
                , Kind.LRC.Info(), 3));
            Surveys.Add(new DuaSurvey(this, SurveyCfg
                , Kind.LDC.Title()
                , Kind.LDC.Info(), 4));
            Surveys.Add(new DuaSurvey(this, SurveyCfg
                , Kind.PAR.Title()
                , Kind.PAR.Info(), 5));

            
            */
        }
        async void OnConnectionChange(object sender, PropertyChangedEventArgs e)
        {
            if (null == sender || "State" != e.PropertyName)
                return;
            if (ConnectionState.Connected == Connection.State)
                await OnConnect();
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
