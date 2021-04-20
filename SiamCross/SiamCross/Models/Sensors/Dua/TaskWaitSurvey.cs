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

        static readonly ushort _ValveTimeSec = 120;
        int SurvayTimeSec => 0 < (Revbit.Value & (1 << 6)) ? 40 : 20;
        int ValveSurvayTimeSec => _ValveTimeSec + SurvayTimeSec;



        TimeSpan _Remain;
        TimeSpan _Total;

        public TaskWaitSurvey(SensorModel sensor)
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
            if (null == Connection)
                return false;

            _Total = TimeSpan.FromSeconds(_ValveTimeSec);
            _Remain = TimeSpan.FromSeconds(_ValveTimeSec);


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

            using (var ctSrc = new CancellationTokenSource(_Remain + TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
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

        async Task WaitAvailability(CancellationToken ct)
        { 
            DuStatus status = DuStatus.NoiseMeasurement;
            while (    DuStatus.NoiseMeasurement == status
                    || DuStatus.WaitingForClick == status
                    || DuStatus.EсhoMeasurement == status
                    || DuStatus.ValvePreparation ==status)
            {
                var startTime = DateTime.Now;
                string remain = $"{_Remain.Hours}:{_Remain.Minutes}:{_Remain.Seconds}"
                    + $" / {_Total.Hours}:{_Total.Minutes}:{_Total.Seconds}";
                await UpdateStatusAsync(ct);
                status = (DuStatus)StatusReg.Value;
                switch (status)
                {
                    default: throw new Exception("Unknown status");
                    case DuStatus.Сompleted:
                    case DuStatus.Empty:
                        InfoEx = $"{remain}\n";
                        break;

                    case DuStatus.NoiseMeasurement:
                    case DuStatus.WaitingForClick:
                    case DuStatus.EсhoMeasurement:
                        InfoEx = $"{remain}\n{DuStatusAdapter.StatusToString(status)}";
                        await Task.Delay(Constants.SecondDelay, ct);
                        break;
                    case DuStatus.ValvePreparation:
                        InfoEx = $"{remain}\n{DuStatusAdapter.StatusToString(status)}, осталось { Timeawt.Value}сек.";
                        await UpdateValvePreparationAsync(ct);
                        break;
                }
                _Remain -= (DateTime.Now - startTime);
            }

        }
        async Task<bool> DoSingleLevelAsync(CancellationToken ct)
        {
            while (4 != StatusReg.Value )
            {
                await WaitAvailability(ct);
                await Task.Delay(Constants.SecondDelay, ct);
                _Remain -= TimeSpan.FromSeconds(1);
            }
                
            return true;
        }
        async Task DoMultiLevelAsync(CancellationToken ct)
        {
            while (true)
            {
                if (0==Kolt.Value)
                    break;
                await WaitAvailability(ct);
                await Connection.TryReadAsync(_CurrentParam, null, ct);
                CalcSurveyRemainTime();

                await Task.Delay(Constants.SecondDelay, ct);
                _Remain -= TimeSpan.FromSeconds(1);
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
                Timeawt.Value = _ValveTimeSec;
                RespResult ret = RespResult.NormalPkg;

                InfoEx = "чтение статуса";
                //ret = await Connection.TryReadAsync(StatusReg, null, linkTsc.Token);
                //if (RespResult.NormalPkg != ret)
                //    return false;
                await WaitAvailability(ct);

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
            return TimeSpan.FromSeconds(_ValveTimeSec + SurvayTimeSec);
        }
        TimeSpan CalcSheduledLevelTotal()
        {
            int timeSec = 0;
            for (int i = 0; i < PerU.Value.Length; ++i)
            {
                var survRemain = Constants.Quantitys[KolUr.Value[i]];
                var survTime = Constants.Periods[PerU.Value[i]] * 60;
                if (survTime < 180)
                    survTime = ValveSurvayTimeSec;
                timeSec += survRemain * survTime;
            }
            var ts = TimeSpan.FromSeconds(timeSec+ SurvayTimeSec);
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
            DuStatus status = (DuStatus)StatusReg.Value;
            switch (status)
            {
                default:
                case DuStatus.Сompleted:
                    break;
                case DuStatus.EсhoMeasurement:
                    return TimeSpan.FromSeconds(SurvayTimeSec);
                case DuStatus.WaitingForClick:
                case DuStatus.Empty:
                case DuStatus.NoiseMeasurement:
                    return TimeSpan.FromSeconds(SurvayTimeSec + _ValveTimeSec);
                case DuStatus.ValvePreparation:
                    return TimeSpan.FromSeconds(SurvayTimeSec + Timeawt.Value);
            }
            return TimeSpan.FromSeconds(1);
        }
        TimeSpan CalcSheduledLevelRemain()
        {
            int timeSec = 0;
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
                    var survTime = Constants.Periods[PerU.Value[i]] * 60;
                    if (survTime < 180)
                        survTime = ValveSurvayTimeSec;
                    timeSec += survRemain * survTime;
                }
            }

            timeSec += Timeost.Value[0] * 3600 + Timeost.Value[1] * 60 + Timeost.Value[2] + SurvayTimeSec + 2;

            var ts = TimeSpan.FromSeconds(timeSec);
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
