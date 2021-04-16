using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Du.Measurement;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskWaitSurvey : BaseSensorTask
    {
        //public readonly MemStruct _Operating;//0x8800
        readonly MemVarUInt8 OpReg = new MemVarUInt8(0x8800);
        readonly MemVarUInt16 StatusReg = new MemVarUInt16(0x8802);

        readonly MemStruct _CurrentParam = new MemStruct(0x8410);
        readonly MemVarByteArray Timeost = new MemVarByteArray(0x8410, new MemValueByteArray(3));
        readonly MemVarUInt8 Interv = new MemVarUInt8(0x8413);
        readonly MemVarUInt16 Kolt = new MemVarUInt16(0x8414);
        readonly MemVarUInt16 Timeawt = new MemVarUInt16(0x8416);

        readonly MemStruct _SurvayParam1 = new MemStruct(0x8008);
        readonly MemVarUInt16 Revbit = new MemVarUInt16(0x8008);
        readonly MemVarUInt8 Vissl = new MemVarUInt8(0x800A);

        readonly MemStruct _SurvayParam2 = new MemStruct(0x8020);
        readonly MemVarUInt8 PerP = new MemVarUInt8(0x8020);
        readonly MemVarUInt8 KolP = new MemVarUInt8(0x8021);
        readonly MemVarByteArray PerU = new MemVarByteArray(0x8022, new MemValueByteArray(5));
        readonly MemVarByteArray KolUr = new MemVarByteArray(0x8027, new MemValueByteArray(5));

        static readonly int _ValvePrepareTimeSec = 120;
        int SurvayTimeSec => 0 < (Revbit.Value & (1 << 6)) ? 36 : 18;



        TimeSpan _Remain;
        TimeSpan _Total;

        public TaskWaitSurvey(ISensor sensor)
            : base(sensor, "Ожидание измерения")
        {
            _CurrentParam.Add(Timeost);
            _CurrentParam.Add(Interv);
            _CurrentParam.Add(Kolt);
            _CurrentParam.Add(Timeawt);

            _SurvayParam1.Add(Revbit);
            _SurvayParam1.Add(Vissl);

            _SurvayParam2.Add(PerP);
            _SurvayParam2.Add(KolP);
            _SurvayParam2.Add(PerU);
            _SurvayParam2.Add(KolUr);

        }
        public override async Task DoBeforeCancelAsync()
        {
            OpReg.Value = 4;
            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
                await Connection.WriteAsync(OpReg, null, ctSrc.Token);
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == Connection || null == Sensor)
                return false;

            _Total = TimeSpan.FromSeconds(_ValvePrepareTimeSec);
            _Remain = TimeSpan.FromSeconds(_ValvePrepareTimeSec);


            if (!await LoadStateAsync(ct))
                return false;
            InfoEx = "определение времени";
            CalcSurveyTotalTime();
            CalcSurveyRemainTime();

            if (_Remain > _Total)
                return false;
            if (0 == _Remain.TotalSeconds)
                return true;
            double progressStart = (double)(1.0 - _Remain.TotalMilliseconds / _Total.TotalMilliseconds);
            InfoEx = "измерение";

            using (var ctSrc = new CancellationTokenSource(_Remain+TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
            using (var timer = CreateProgressTimer(_Remain, (float)progressStart))
            {
                switch (Vissl.Value)
                {
                    default:
                    case 1:
                    case 2:
                        await DoSingleLevelAsync(linkTsc.Token);
                        break;
                    case 3:
                    case 4:
                        await DoMultiLevelAsync(linkTsc.Token);
                        break;
                    case 5:
                        await DoMultiPressureAsync(linkTsc.Token);
                        break;
                }
            }
            return true;
        }
        async Task<bool> DoSingleLevelAsync(CancellationToken ct)
        {
            Timeawt.Value = 120;
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            while (DuMeasurementStatus.Сompleted != status)
            {
                _Remain -= TimeSpan.FromSeconds(1);
                string remain = $"{_Remain.Hours}:{_Remain.Minutes}:{_Remain.Seconds}"
                    + $" / {_Total.Hours}:{_Total.Minutes}:{_Total.Seconds}";

                await Task.Delay(Constants.SecondDelay, ct);
                await UpdateStatusAsync(ct);
                status = (DuMeasurementStatus)StatusReg.Value;
                switch (status)
                {
                    default: throw new Exception("Unknown status");
                    case DuMeasurementStatus.Сompleted:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.WaitingForClick:
                    case DuMeasurementStatus.Empty:
                    case DuMeasurementStatus.NoiseMeasurement:
                        InfoEx = $"{remain}\n{DuStatusAdapter.StatusToString(status)}";
                        break;
                    case DuMeasurementStatus.ValvePreparation:
                        InfoEx = $"{remain}\n{DuStatusAdapter.StatusToString(status)}, осталось { Timeawt.Value}сек.";
                        await UpdateValvePreparationAsync(ct);
                        break;
                }
            }
            return true;
        }
        async Task DoMultiLevelAsync(CancellationToken ct)
        {
            while (true)
            {
                if (Kolt.Value == 0 && (Interv.Value == 4 || 0 == KolUr.Value[Interv.Value + 1]))
                    break;
                await DoSingleLevelAsync(ct);
                await Connection.TryReadAsync(_CurrentParam, null, ct);
                CalcSurveyRemainTime();
            }
        }
        async Task DoMultiPressureAsync(CancellationToken ct)
        {
            while (true)
            {

                string remain = $"{_Remain.Hours}:{_Remain.Minutes}:{_Remain.Seconds}"
                    + $" / {_Total.Hours}:{_Total.Minutes}:{_Total.Seconds}";
                InfoEx = $"{remain}\nзамеров давления осталось: {Kolt.Value}";
                if (Kolt.Value == 0)
                    break;
                await Task.Delay(Constants.SecondDelay * 5, ct);
                await Connection.TryReadAsync(_CurrentParam, null, ct);
                CalcSurveyRemainTime();
            }
        }

        async Task<bool> LoadStateAsync(CancellationToken ct)
        {
            
            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
            {
                if (!await CheckConnectionAsync(linkTsc.Token))
                    return false;

                RespResult ret = RespResult.NormalPkg;

                InfoEx = "чтение статуса";

                do
                {
                    //ret = await Connection.TryReadAsync(StatusReg, null, linkTsc.Token);
                    //if (RespResult.NormalPkg != ret)
                    //    return false;
                    await DoSingleLevelAsync(ct);
                }
                while (4 > StatusReg.Value);

                InfoEx = "чтение состояния";
                ret = await Connection.TryReadAsync(_CurrentParam, null, linkTsc.Token);
                if (RespResult.NormalPkg != ret)
                    return false;
                    
                InfoEx = "чтение параметров 1";
                ret = await Connection.TryReadAsync(_SurvayParam1, null, linkTsc.Token);
                if (RespResult.NormalPkg != ret)
                    return false;

                InfoEx = "чтение параметров 2";
                ret = await Connection.TryReadAsync(_SurvayParam2, null, linkTsc.Token);
                if (RespResult.NormalPkg != ret)
                    return false;
                return true;



            }
        }

        void CalcSurveyTotalTime()
        {
            switch (Vissl.Value)
            {
                default:
                case 1:
                case 2:
                    _Total = CalcNonSheduledTotal();
                    break;
                case 3:
                case 4:
                    _Total = CalcSheduledLevelTotal();
                    break;
                case 5:
                    _Total = CalcSheduledPressureTotal();
                    break;
            }
        }
        void CalcSurveyRemainTime()
        {
            switch (Vissl.Value)
            {
                default:
                case 1:
                case 2:
                    _Remain = CalcNonSheduledRemain();
                    break;
                case 3:
                case 4:
                    _Remain = CalcSheduledLevelRemain();
                    break;
                case 5:
                    _Remain = CalcSheduledPressureRemain();
                    break;
            }
        }
        TimeSpan CalcNonSheduledTotal()
        {
            return TimeSpan.FromSeconds(_ValvePrepareTimeSec + SurvayTimeSec);
        }
        TimeSpan CalcSheduledLevelTotal()
        {
            int surveysTimeSec = 0;
            for (int i = 0; i < PerU.Value.Length; ++i)
            {
                var survRemain = Constants.Quantitys[KolUr.Value[i]];
                var survTime = Constants.Quantitys[KolUr.Value[i]] * 60;
                if (survTime < 180)
                    survTime = _ValvePrepareTimeSec + SurvayTimeSec;
                surveysTimeSec += survRemain * survTime;
            }
            var ts = TimeSpan.FromSeconds(surveysTimeSec);
            return ts;
        }
        TimeSpan CalcSheduledPressureTotal()
        {
            var survRemain = Constants.Quantitys[KolP.Value];
            var surveysTimeSec = survRemain * Constants.Periods[PerP.Value] * 60;
            var ts = TimeSpan.FromSeconds(surveysTimeSec);
            return ts;
        }
        TimeSpan CalcNonSheduledRemain()
        {
            DuMeasurementStatus status = (DuMeasurementStatus)StatusReg.Value;
            switch (status)
            {
                default:
                case DuMeasurementStatus.Сompleted:
                    break;
                case DuMeasurementStatus.EсhoMeasurement:
                    return TimeSpan.FromSeconds(SurvayTimeSec);
                case DuMeasurementStatus.WaitingForClick:
                case DuMeasurementStatus.Empty:
                case DuMeasurementStatus.NoiseMeasurement:
                    return TimeSpan.FromSeconds(SurvayTimeSec + _ValvePrepareTimeSec);
                case DuMeasurementStatus.ValvePreparation:
                    return TimeSpan.FromSeconds(SurvayTimeSec + Timeawt.Value );
            }
            return TimeSpan.FromSeconds(1);
        }
        TimeSpan CalcSheduledLevelRemain()
        {
            int surveysTimeSec = 0;
            for (int i = Interv.Value; i < PerU.Value.Length; ++i)
            {
                int survRemain;
                if (i == Interv.Value)
                {
                    if (0 < Kolt.Value)
                        survRemain = Kolt.Value - 1;
                    else
                        survRemain = 0;
                }
                else
                    survRemain = Constants.Quantitys[KolUr.Value[i]];

                if (0 < survRemain)
                {
                    var survTime = Constants.Quantitys[KolUr.Value[i]] * 60;
                    if (survTime < 180)
                        survTime = _ValvePrepareTimeSec + SurvayTimeSec;
                    surveysTimeSec += survRemain * survTime;
                }
            }
            surveysTimeSec += Timeost.Value[0] * 3600 + Timeost.Value[1] * 60 + Timeost.Value[2];
            var ts = TimeSpan.FromSeconds(surveysTimeSec);
            return ts;
        }
        TimeSpan CalcSheduledPressureRemain()
        {
            int survRemain;
            if (0 < Kolt.Value)
                survRemain = Kolt.Value - 1;
            else
                survRemain = 0;
            var surveysTimeSec = survRemain * Constants.Periods[PerP.Value] * 60;
            surveysTimeSec += Timeost.Value[0] * 3600 + Timeost.Value[1] * 60 + Timeost.Value[2];
            var ts = TimeSpan.FromSeconds(surveysTimeSec);
            return ts;

        }

        async Task UpdateStatusAsync(CancellationToken ct)
        {
            try
            {
                await Connection.ReadAsync(StatusReg, null, ct);
            }
            catch (ProtocolException ex)
            {
                LogException(ex);
            }

        }
        async Task UpdateValvePreparationAsync(CancellationToken ct)
        {
            try
            {
                await Connection.ReadAsync(Timeawt, null, ct);
            }
            catch (ProtocolException ex)
            {
                LogException(ex);
            }
        }

    }
}
