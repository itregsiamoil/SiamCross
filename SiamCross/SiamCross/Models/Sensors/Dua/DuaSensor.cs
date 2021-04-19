using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dua.Surveys;
using SiamCross.ViewModels.Dua;
using SiamCross.ViewModels.Dua.Survey;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaSensor : BaseSensor2
    {
        public readonly MemStruct _CurrentParam;//0x8400
        public readonly MemVarUInt16 BatteryVoltage;
        public readonly MemVarUInt16 ТempC;
        public readonly MemVarInt16 Pressure;



        public DuaSensor(IProtocolConnection conn, DeviceInfo deviceInfo)
            : base(conn, deviceInfo)
        {
            Connection.AdditioonalTimeout = 2000;

            Model.Storage = new DuaStorage(Model);
            StorageVM = new DuaStorageVM(this);


            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            ТempC = _CurrentParam.Add(new MemVarUInt16(nameof(ТempC)));
            Pressure = _CurrentParam.Add(new MemVarInt16(nameof(Pressure)));

            Model.SurveyCfg = new DuaSurveyCfg(this);

            {
                var model = new DuaSurvey(this, Model.SurveyCfg
                    , "Статический уровень"
                    , "уровень жидкости в спокойном состоянии", 1);
                Model.Surveys.Add(model);
                var vm = new SurveyVM(this, model);
                SurveysVM.SurveysCollection.Add(vm);
            }
            {
                var model = new DuaSurvey(this, Model.SurveyCfg
                    , "Динамический уровень"
                    , "уровень жидкости, наблюдаемый при откачке", 2);
                Model.Surveys.Add(model);
                var vm = new SurveyVM(this, model);
                SurveysVM.SurveysCollection.Add(vm);
            }
            {
                var model = new DuaSurvey(this, Model.SurveyCfg
                    , "КВУ"
                    , "кривая восстановления уровня", 3);
                Model.Surveys.Add(model);
                var vm = new SurveyVM(this, model);
                SurveysVM.SurveysCollection.Add(vm);
            }
            {
                var model = new DuaSurvey(this, Model.SurveyCfg
                    , "КПУ"
                    , "кривая падения уровня", 4);
                Model.Surveys.Add(model);
                var vm = new SurveyVM(this, model);
                SurveysVM.SurveysCollection.Add(vm);
            }
            {
                var model = new DuaSurvey(this, Model.SurveyCfg
                    , "АРД"
                    , "автоматическая регистрация давления", 5);
                Model.Surveys.Add(model);
                var vm = new SurveyVM(this, model);
                SurveysVM.SurveysCollection.Add(vm);
            }



            //FactoryConfigVM = new FactoryConfigVM(this);
            //UserConfigVM = new UserConfigVM(this);
            //StateVM = new StateVM(this);



        }

        public override void OnConnect()
        {
            var manager = Model.Manager;
            var taskWaitSurvey = new TaskWaitSurvey(Model);
            Task.Run(() => manager.Execute(taskWaitSurvey));
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
