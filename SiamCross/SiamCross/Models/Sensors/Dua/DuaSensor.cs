using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dua.Surveys;
using SiamCross.ViewModels.Dua;
using SiamCross.ViewModels.Dua.Survey;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaSensorModel : SensorModel
    {
        public DuaSensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
           : base(conn, deviceInfo)
        {
            Connection.AdditioonalTimeout = 2000;

            Position.TaskLoad = new TaskPositionLoad(Position);
            Position.TaskSave = new TaskPositionSave(Position);

            Storage = new DuaStorage(this);
            SurveyCfg = new DuaSurveyCfg(this);

            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LStatic));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LDynamic));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LRC));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.LDC));
            Surveys.Add(new DuaSurvey(this, SurveyCfg, Kind.PAR));

            Connection.PropertyChanged += OnConnectionChange;
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
            await (Surveys[0].CmdWait as AsyncCommand)?.ExecuteAsync();
            await (Position.CmdLoad as AsyncCommand)?.ExecuteAsync();
        }
        public override void Dispose()
        {
            Connection.PropertyChanged -= OnConnectionChange;
            base.Dispose();
        }

    }


    public class DuaSensor : BaseSensor2
    {
        public readonly MemStruct _CurrentParam;//0x8400
        public readonly MemVarUInt16 BatteryVoltage;
        public readonly MemVarUInt16 ТempC;
        public readonly MemVarInt16 Pressure;



        public DuaSensor(SensorModel model)
            : base(model)
        {
            StorageVM = new DuaStorageVM(this);

            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            ТempC = _CurrentParam.Add(new MemVarUInt16(nameof(ТempC)));
            Pressure = _CurrentParam.Add(new MemVarInt16(nameof(Pressure)));

            foreach (var surveyModel in Model.Surveys)
            {
                var vm = new SurveyVM(this, surveyModel as DuaSurvey);
                SurveysVM.SurveysCollection.Add(vm);
            }


            //FactoryConfigVM = new FactoryConfigVM(this);
            //UserConfigVM = new UserConfigVM(this);
            //StateVM = new StateVM(this);



        }

        public override async Task<bool> QuickReport(CancellationToken cancelToken)
        {
            try
            {
                cancelToken.ThrowIfCancellationRequested();

                RespResult ret = await Connection.ReadAsync(_CurrentParam);

                Battery = (BatteryVoltage.Value / 10.0).ToString();
                Temperature = (ТempC.Value / 10.0).ToString();
                //_reportBuilder.Load = LoadChanel.Value.ToString();
                //_reportBuilder.Acceleration = AccelerationChanel.Value.ToString();
                //SensorData.Status = _reportBuilder.GetReport();

                Status = $"{Resource.Pressure}: "
                    + Pressure.Value / 10.0 + $" ({Resource.KGFCMUnits})";

                ChangeNotify(nameof(Battery));
                ChangeNotify(nameof(Temperature));
                ChangeNotify(nameof(Status));
                ScannedDeviceInfo.Device.DeviceData["Battery"] = Battery;
                ScannedDeviceInfo.Device.DeviceData["Temperature"] = Temperature;
                ScannedDeviceInfo.Device.DeviceData["Status"] = Status;
                return true;
            }
            catch (ProtocolException)
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
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
