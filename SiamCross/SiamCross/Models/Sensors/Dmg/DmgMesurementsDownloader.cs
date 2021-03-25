using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Dmg.Ddin2.Measurement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class DmgMesurementsDownloader : IMeasurementsDownloader
    {
        private ISensor _Sensor;
        private IProtocolConnection _Connection;

        readonly byte[] _currentDynGraph = new byte[1000 * 2];
        readonly byte[] _currentAccelerationGraph = new byte[1000 * 2];

        readonly MemStruct _SurvayParam;
        readonly MemVarUInt16 Rod;
        readonly MemVarUInt32 DynPeriod;
        readonly MemVarUInt16 ApertNumber;
        readonly MemVarUInt16 Imtravel;
        readonly MemVarUInt16 ModelPump;

        readonly MemStruct _Operating;
        readonly MemVarUInt16 CtrlReg;
        readonly MemVarUInt16 StatReg;
        readonly MemVarUInt32 ErrorReg;

        readonly MemStruct _Report;
        readonly MemVarUInt16 MaxWeight;
        readonly MemVarUInt16 MinWeight;
        readonly MemVarUInt16 Travel;
        readonly MemVarUInt16 Period;
        readonly MemVarUInt16 Step;
        readonly MemVarUInt16 WeightDiscr;
        readonly MemVarUInt16 TimeDiscr;
        public DmgMesurementsDownloader(ISensor sensor)
        {
            _Sensor = sensor;
            _Connection = sensor.Connection;

            _SurvayParam = new MemStruct(0x8000);
            Rod = _SurvayParam.Add(new MemVarUInt16(nameof(Rod)));
            DynPeriod = _SurvayParam.Add(new MemVarUInt32(nameof(DynPeriod)));
            ApertNumber = _SurvayParam.Add(new MemVarUInt16(nameof(ApertNumber)));
            Imtravel = _SurvayParam.Add(new MemVarUInt16(nameof(Imtravel)));
            ModelPump = _SurvayParam.Add(new MemVarUInt16(nameof(ModelPump)));

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
        }

        public async Task Clear()
        {
            CtrlReg.Value = 0x02;
            await _Connection.ReadAsync(StatReg);
        }

        bool NeedRetry(RespResult result)
        {
            switch (result)
            {
                case RespResult.ErrorPkg:
                case RespResult.NormalPkg: return false;
                default:
                case RespResult.ErrorUnknown:
                case RespResult.ErrorConnection:
                case RespResult.ErrorSending:
                case RespResult.ErrorTimeout:
                case RespResult.ErrorCrc:
                    return true;
            }
        }
        public async Task<RespResult> Update()
        {
            if (!_Sensor.Activate)
                _Sensor.Activate = true;
            RespResult ret = RespResult.ErrorTimeout;
            for (int i = 0; i < 3; ++i)
            {
                ret = await _Connection.TryReadAsync(StatReg);
                if (!NeedRetry(ret))
                    break;
            }
            return ret;
        }

        public int Aviable()
        {
            switch (StatReg.Value)
            {
                default: return 0;
                case 0x04: return 1;
                case 0x05: return 1;
            }
        }
        public async Task<IReadOnlyList<object>> Download(uint begin, uint end
            , Action<float> onStepProgress = null, Action<string> onStepInfo = null)
        {
            onStepProgress?.Invoke(0.01f);
            onStepInfo?.Invoke("Download SurvayParam");
            await _Connection.ReadAsync(_SurvayParam);

            onStepProgress?.Invoke(0.02f);
            onStepInfo?.Invoke("DownloadHeader start");
            await _Connection.ReadAsync(_Report);
            onStepProgress?.Invoke(0.05f);

            onStepInfo?.Invoke("DownloadMeasurement ");
            RespResult ret = await _Connection.ReadMemAsync(0x81000000
                , 1000 * 2, _currentDynGraph, 0, onStepProgress);



            DmgBaseMeasureReport _report = new DmgBaseMeasureReport(
                MaxWeight.Value
                , MinWeight.Value
                , Travel.Value
                , Period.Value
                , Step.Value
                , WeightDiscr.Value
                , TimeDiscr.Value);

            var sp = new MeasurementSecondaryParameters(
                _Sensor.Name
                , Resource.Dynamogram
                , string.IsNullOrEmpty(_Sensor.PositionVM.Field) ? "0" : _Sensor.PositionVM.Field
                , string.IsNullOrEmpty(_Sensor.PositionVM.Well) ? "0" : _Sensor.PositionVM.Well
                , string.IsNullOrEmpty(_Sensor.PositionVM.Bush) ? "0" : _Sensor.PositionVM.Bush
                , string.IsNullOrEmpty(_Sensor.PositionVM.Shop) ? "0" : _Sensor.PositionVM.Shop
                , 0.0
                , string.Empty
                , _Sensor.Battery
                , _Sensor.Temperature
                , _Sensor.Firmware
                , string.Empty
                );

            Ddin2MeasurementData measurement =
               new Ddin2MeasurementData(
                   _report,
                   (short)ApertNumber.Value,
                   (short)ModelPump.Value,
                   (short)Rod.Value,
                   _currentDynGraph.ToList(),
                   DateTime.Now,
                   sp,
                   _currentAccelerationGraph.ToList(),
                   BitConverter.GetBytes(ErrorReg.Value));

            //double[,] dynGraphPoints = DgmConverter.GetXYs(measurement.DynGraph.ToList(),
            //    measurement.Report.Step, measurement.Report.WeightDiscr);
            //measurement.DynGraphPoints = dynGraphPoints;

            var data_list = new List<Ddin2MeasurementData>();
            data_list.Add(measurement);
            return data_list;
        }
    }
}
