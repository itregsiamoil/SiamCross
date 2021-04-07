using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;
using SiamCross.ViewModels.Dmg;
using SiamCross.ViewModels.Dmg.Survey;
using SiamCross.ViewModels.MeasurementViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public abstract class DmgBaseSensor : BaseSensor2
    {
        public readonly MemStruct _SurvayParam;
        public readonly MemVarUInt16 Rod;
        public readonly MemVarUInt32 DynPeriod;
        public readonly MemVarUInt16 ApertNumber;
        public readonly MemVarUInt16 Imtravel;
        public readonly MemVarUInt16 ModelPump;
        public readonly MemStruct _NonvolatileParam;
        public readonly MemVarFloat Nkp;
        public readonly MemVarFloat Rkp;
        public readonly MemVarFloat ZeroG;
        public readonly MemVarFloat PositiveG;
        public readonly MemVarUInt32 EnableInterval;
        public readonly MemVarFloat ZeroOffset;
        public readonly MemVarFloat SlopeRatio;
        public readonly MemVarFloat NegativeG;
        public readonly MemVarUInt16 SleepDisable;
        public readonly MemVarUInt16 SleepTimeout;
        public readonly MemStruct _CurrentParam;
        public readonly MemVarUInt16 BatteryVoltage;
        public readonly MemVarInt16 Тemperature;
        public readonly MemVarFloat LoadChanel;
        public readonly MemVarFloat AccelerationChanel;
        public readonly MemStruct _Operating;
        public readonly MemVarUInt16 CtrlReg;
        public readonly MemVarUInt16 StatReg;
        public readonly MemVarUInt32 ErrorReg;

        public readonly MemStruct _Report;
        public readonly MemVarUInt16 MaxWeight;
        public readonly MemVarUInt16 MinWeight;
        public readonly MemVarUInt16 Travel;
        public readonly MemVarUInt16 Period;
        public readonly MemVarUInt16 Step;
        public readonly MemVarUInt16 WeightDiscr;
        public readonly MemVarUInt16 TimeDiscr;


        public DmgBaseSensor(IProtocolConnection conn, ScannedDeviceInfo dev_info)
            : base(conn, dev_info)
        {
            _SurvayParam = new MemStruct(0x8000);
            Rod = _SurvayParam.Add(new MemVarUInt16(nameof(Rod)));
            DynPeriod = _SurvayParam.Add(new MemVarUInt32(nameof(DynPeriod)));
            ApertNumber = _SurvayParam.Add(new MemVarUInt16(nameof(ApertNumber)));
            Imtravel = _SurvayParam.Add(new MemVarUInt16(nameof(Imtravel)));
            ModelPump = _SurvayParam.Add(new MemVarUInt16(nameof(ModelPump)));

            _NonvolatileParam = new MemStruct(0x8100);
            Nkp = _NonvolatileParam.Add(new MemVarFloat(nameof(Nkp)));
            Rkp = _NonvolatileParam.Add(new MemVarFloat(nameof(Rkp)));
            ZeroG = _NonvolatileParam.Add(new MemVarFloat(nameof(ZeroG)));
            PositiveG = _NonvolatileParam.Add(new MemVarFloat(nameof(PositiveG)));
            EnableInterval = _NonvolatileParam.Add(new MemVarUInt32(nameof(EnableInterval)));
            ZeroOffset = _NonvolatileParam.Add(new MemVarFloat(nameof(ZeroOffset)));
            SlopeRatio = _NonvolatileParam.Add(new MemVarFloat(nameof(SlopeRatio)));
            NegativeG = _NonvolatileParam.Add(new MemVarFloat(nameof(NegativeG)));
            SleepDisable = _NonvolatileParam.Add(new MemVarUInt16(nameof(SleepDisable)));
            SleepTimeout = _NonvolatileParam.Add(new MemVarUInt16(nameof(SleepTimeout)));

            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            Тemperature = _CurrentParam.Add(new MemVarInt16(nameof(Тemperature)));
            LoadChanel = _CurrentParam.Add(new MemVarFloat(nameof(LoadChanel)));
            AccelerationChanel = _CurrentParam.Add(new MemVarFloat(nameof(AccelerationChanel)));

            _Operating = new MemStruct(0x8800);
            CtrlReg = _Operating.Add(new MemVarUInt16(nameof(CtrlReg)));
            StatReg = _Operating.Add(new MemVarUInt16(nameof(StatReg)));
            ErrorReg = _Operating.Add(new MemVarUInt32(nameof(ErrorReg)));

            _Report = new MemStruct(0x80000000);
            MaxWeight = _Report.Add(new MemVarUInt16(nameof(MaxWeight)));
            MinWeight = _Report.Add(new MemVarUInt16(nameof(MinWeight)));
            Travel = _Report.Add(new MemVarUInt16(nameof(Travel)));
            Period = _Report.Add(new MemVarUInt16(nameof(Period)));
            Step = _Report.Add(new MemVarUInt16(nameof(Step)));
            WeightDiscr = _Report.Add(new MemVarUInt16(nameof(WeightDiscr)));
            TimeDiscr = _Report.Add(new MemVarUInt16(nameof(TimeDiscr)));

            var surveys = new List<SurveyVM>();
            Surveys = surveys;

            var sur1 = new DynamogrammVM(this
                , "Динамограмма"
                , "long long description");
            surveys.Add(sur1);

            var sur2 = new SurveyVM(this
                , "Тест клапанов"
                , "long long description");
            surveys.Add(sur2);

            var sur3 = new SurveyVM(this
                , "Вес штанг"
                , "long long description");
            surveys.Add(sur3);


            Model.Storage = new DmgStorage(this);
            StorageVM = new DmgStorageVM(this);

            //FactoryConfigVM = new FactoryConfigVM(this);
            //UserConfigVM = new UserConfigVM(this);
            //StateVM = new StateVM(this);

            SurveysVM = new SurveysCollectionVM(this);

        }



        public override async Task<bool> QuickReport(CancellationToken cancelToken)
        {
            try
            {
                cancelToken.ThrowIfCancellationRequested();
                RespResult ret = await Connection.ReadAsync(_CurrentParam);

                Battery = (BatteryVoltage.Value / 10.0).ToString();
                Temperature = (Тemperature.Value / 10.0).ToString();
                //_reportBuilder.Load = LoadChanel.Value.ToString();
                //_reportBuilder.Acceleration = AccelerationChanel.Value.ToString();
                //SensorData.Status = _reportBuilder.GetReport();
                Status = GetLoadSting() + "\n" + GetAccelerationSting();

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
        public async Task<bool> KillosParametersQuery(CancellationToken cancelToken)
        {
            RespResult ret;
            try
            {
                cancelToken.ThrowIfCancellationRequested();
                ret = await Connection.ReadAsync(_NonvolatileParam);
                //_reportBuilder.SensitivityLoad = Rkp.Value.ToString();
                //_reportBuilder.ZeroOffsetLoad = Nkp.Value.ToString();
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
        public override async Task<bool> PostConnectInit(CancellationToken cancelToken)
        {
            await Connection.Connect();
            Status = Resource.ConnectedStatus;
            return (await UpdateFirmware(cancelToken) && await KillosParametersQuery(cancelToken));
        }

        public string GetLoadSting()
        {
            float load_mv = LoadChanel.Value;
            string ret = $"{Resource.Load}: {Math.Round(load_mv, 2)}, {Resource.MilliVoltsUnits}";
            if (Rkp.Value != 0)
            {
                float load_kg = (LoadChanel.Value - Nkp.Value) / Rkp.Value;
                ret += $" \t {Math.Round(load_kg, 2)}, {Resource.Kilograms}";
            }
            return ret;
        }
        public string GetAccelerationSting()
        {
            float _acceleration = AccelerationChanel.Value;
            string ret = $"{Resource.Acceleration}: {Math.Round(_acceleration, 2)}, {Resource.MilliVoltsUnits}";
            return ret;
        }

    }//DmgBaseSensor
}
