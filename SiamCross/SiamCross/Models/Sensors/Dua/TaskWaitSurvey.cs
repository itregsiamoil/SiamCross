using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Du.Measurement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskWaitSurvey : BaseSensorTask
    {
        public readonly List<MemVar> Reg = new List<MemVar>();
        
        public readonly MemVarUInt8 OpReg = new MemVarUInt8(0x8800);
        public readonly MemVarUInt16 StatusReg = new MemVarUInt16(0x8802);

        public readonly MemVarByteArray Timeost = new MemVarByteArray(0x8410, new MemValueByteArray(3));
        public readonly MemVarUInt8 Interv = new MemVarUInt8(0x8413);
        public readonly MemVarUInt16 Kolt = new MemVarUInt16(0x8414);
        public readonly MemVarUInt16 Timeawt = new MemVarUInt16(0x8416);

        public readonly MemVarUInt16 Revbit = new MemVarUInt16(0x8008);
        public readonly MemVarUInt8 Vissl = new MemVarUInt8(0x800A);
        
        public readonly MemVarUInt8 PerP = new MemVarUInt8(0x8020);
        public readonly MemVarUInt8 KolP = new MemVarUInt8(0x8021);
        public readonly MemVarByteArray PerU = new MemVarByteArray(0x8022, new MemValueByteArray(5));
        public readonly MemVarByteArray KolUr = new MemVarByteArray(0x8027, new MemValueByteArray(5));

        static readonly int _TimeoutValvePrepare = 120000;
        static readonly int _TimeoutSurvay3000 = 20000;
        static readonly int _TimeoutSurvay6000 = 2 * _TimeoutSurvay3000;

        TimeSpan _Remain;
        TimeSpan _Total;

        public TaskWaitSurvey(ISensor sensor)
            : base(sensor, "Ожидание измерения")
        {
            Reg.Add(OpReg);
            Reg.Add(StatusReg);

            Reg.Add(Timeost);
            Reg.Add(Interv);
            Reg.Add(Kolt);
            Reg.Add(Timeawt);

            Reg.Add(Revbit);
            Reg.Add(Vissl);
            Reg.Add(PerP);
            Reg.Add(KolP);
            Reg.Add(PerU);
            Reg.Add(KolUr);
        }
        public override Task DoBeforeCancelAsync()
        {
            OpReg.Value = 4;
            return Connection.WriteAsync(OpReg, null, _Cts.Token);
        }

        public override async Task<bool> DoExecute()
        {
            if (null == Connection || null == Sensor)
                return false;
            if (!await CheckConnectionAsync())
                return false;

            if (!await RetryExecAsync(3, LoadState))
                return false;
            CalcSurveyTime();
            _Cts.CancelAfter(_Remain);

            double progressStart = (double)_Remain.TotalMilliseconds / _Total.TotalMilliseconds;
            int totalmsec = (int)_Remain.TotalMilliseconds;
            InfoEx = "измерение";
            using (var timer = CreateProgressTimer(totalmsec, (float)progressStart))
                return await ProcessSurvey();
        }

        private async Task<bool> ProcessSurvey()
        {
            Timeawt.Value = 120;
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            while (DuMeasurementStatus.Сompleted != status)
            {
                _Remain -= TimeSpan.FromSeconds(1);
                string remain = $"осталось: {_Remain.Hours}:{_Remain.Minutes}:{_Remain.Seconds}";

                _Cts.Token.ThrowIfCancellationRequested();
                await Task.Delay(Constants.SecondDelay, _Cts.Token);
                await UpdateStatus();
                status = (DuMeasurementStatus)StatusReg.Value;
                switch (status)
                {
                    default: throw new Exception("Unknown status");
                    case DuMeasurementStatus.Сompleted:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.WaitingForClick:
                    case DuMeasurementStatus.Empty:
                    case DuMeasurementStatus.NoiseMeasurement:
                        InfoEx = $"{remain}\nклапан: {DuStatusAdapter.StatusToString(status)}";
                        break;
                    case DuMeasurementStatus.ValvePreparation:
                        InfoEx = $"{remain}\nклапан: {DuStatusAdapter.StatusToString(status)}, осталось { Timeawt.Value}сек.";
                        await UpdateValvePreparation();
                        break;
                }
            }
            return true;
        }

        private async Task<bool> LoadState()
        {
            InfoEx = "инициализация";
            RespResult ret = RespResult.NormalPkg;

            ret = await Connection.TryReadAsync(StatusReg, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(Timeost, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;

            ret = await Connection.TryReadAsync(Interv, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(Kolt, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(Timeawt, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;

            ret = await Connection.TryReadAsync(Revbit, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(Vissl, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(PerP, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(KolP, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(PerU, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            ret = await Connection.TryReadAsync(KolUr, null, _Cts.Token);
            if (RespResult.NormalPkg != ret)
                return false;
            
            /*
            foreach (var r in Reg)
            {
                RespResult ret = await Connection.TryReadAsync(r, null, _Cts.Token);
                if (RespResult.NormalPkg != ret)
                    return false;
            }
            */
            return true;
        }

        void CalcSurveyTime()
        {
            InfoEx = "определение времени";
            switch (Vissl.Value)
            {
                default:
                case 1:
                case 2:
                    _Remain = CalcNonSheduledRemain();
                    _Total = CalcNonSheduledTotal();
                    break;
                case 3:
                case 4:
                    _Remain = CalcSheduledLevelRemain();
                    _Total = CalcSheduledLevelTotal();
                    break;
                case 5:
                    _Remain = CalcSheduledPressureRemain();
                    _Total = CalcSheduledPressureTotal();
                    break;
            }
        }
        TimeSpan CalcNonSheduledTotal()
        {
            return TimeSpan.FromMilliseconds(_TimeoutSurvay6000 + _TimeoutValvePrepare);
        }
        TimeSpan CalcSheduledLevelTotal()
        {
            int surveysTimeSec = 0;
            for(int i=0; i< PerU.Value.Length; ++i)    
            {
                var survRemain = Constants.Quantitys[KolUr.Value[i]];
                surveysTimeSec += survRemain * Constants.Periods[PerU.Value[i]];
            }
            var ts = TimeSpan.FromSeconds(surveysTimeSec);
            return ts;
        }
        TimeSpan CalcSheduledPressureTotal()
        {
            var survRemain = Constants.Quantitys[KolP.Value];
            var surveysTimeSec = survRemain * Constants.Periods[PerP.Value];
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
                    return TimeSpan.FromMilliseconds(_TimeoutSurvay6000);
                case DuMeasurementStatus.WaitingForClick:
                case DuMeasurementStatus.Empty:
                case DuMeasurementStatus.NoiseMeasurement:
                    return TimeSpan.FromMilliseconds(_TimeoutSurvay6000 + _TimeoutValvePrepare);
                case DuMeasurementStatus.ValvePreparation:
                    return TimeSpan.FromMilliseconds(_TimeoutSurvay6000 + Timeawt.Value*1000);
            }
            return TimeSpan.FromMilliseconds(1);
        }
        TimeSpan CalcSheduledLevelRemain()
        {
            int surveysTimeSec = 0;
            var i = Interv.Value;
            while(i< PerU.Value.Length)
            {
                var survRemain  = (i == Interv.Value)? Kolt.Value : Constants.Quantitys[KolUr.Value[i]];
                surveysTimeSec += survRemain * Constants.Periods[PerU.Value[i]];
            }

            surveysTimeSec += Timeost.Value[0] * 3600 + Timeost.Value[1] * 60 + Timeost.Value[2];

            var ts = TimeSpan.FromSeconds(surveysTimeSec);
            ts += CalcNonSheduledRemain();
            return ts;
        }
        TimeSpan CalcSheduledPressureRemain()
        {
            var survRemain = Constants.Quantitys[KolP.Value];
            var surveysTimeSec = survRemain * Constants.Periods[PerP.Value];
            surveysTimeSec += Timeost.Value[0] * 3600 + Timeost.Value[1] * 60 + Timeost.Value[2];

            var ts = TimeSpan.FromSeconds(surveysTimeSec);
            ts += CalcNonSheduledRemain();
            return ts;

        }

        async Task UpdateStatus()
        {
            try
            {
                await Connection.ReadAsync(StatusReg, null, _Cts.Token);
            }
            catch (ProtocolException ex)
            {
                LogException(ex);
            }

        }
        async Task UpdateValvePreparation()
        {
            try
            {
                await Connection.ReadAsync(Timeawt, null, _Cts.Token);
            }
            catch (ProtocolException ex)
            {
                LogException(ex);
            }
        }

    }
}
