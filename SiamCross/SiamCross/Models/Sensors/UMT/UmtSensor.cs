using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Umt.Surveys;
using SiamCross.ViewModels.Umt;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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

            SurveyCfg = new SurveyCfg(this);
            Storage = new Storage(this);

            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.Static));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.Dynamic));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.PeriodicStatic));
            Surveys.Add(new UmtSurvey(this, SurveyCfg, Kind.PeriodycDynamic));
        }
    }

    public class UmtSensorVM : BaseSensor2
    {
        readonly MemStruct _CurrentParam;//0x8400
        readonly MemVarFloat Pressure;
        readonly MemVarFloat ТempInt;
        readonly MemVarFloat ТempExt;
        readonly MemVarFloat Acc;

        public UmtSensorVM(SensorModel model)
            : base(model)
        {
            _CurrentParam = new MemStruct(0x8400);
            Pressure = _CurrentParam.Add(new MemVarFloat(nameof(Pressure)));
            ТempInt = _CurrentParam.Add(new MemVarFloat(nameof(ТempInt)));
            ТempExt = _CurrentParam.Add(new MemVarFloat(nameof(ТempExt)));
            Acc = _CurrentParam.Add(new MemVarFloat(nameof(Acc)));

            StorageVM = new StorageVM(this);

            foreach (var surveyModel in Model.Surveys)
            {
                var vm = new SurveyVM(this, surveyModel as UmtSurvey);
                SurveysVM.SurveysCollection.Add(vm);
            }

            Model.ConnHolder.PropertyChanged += OnHolderChange;
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

        public override async Task<bool> QuickReport(CancellationToken cancelToken)
        {
            try
            {
                cancelToken.ThrowIfCancellationRequested();

                RespResult ret = await Connection.ReadAsync(_CurrentParam);

                Battery = (Acc.Value ).ToString("N2");
                Temperature = (ТempInt.Value).ToString("N2");

                var press_str = (Pressure.Value).ToString("N2");
                var exttemp_str = (ТempExt.Value).ToString("N2");

                Status =
                    $"{Resource.Pressure}: " + press_str + $" ({Resource.KGFCMUnits})"
                    + "\nТемп.зонда: " + exttemp_str + $" ({Resource.DegCentigradeUnits})";

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
