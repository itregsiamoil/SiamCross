using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Umt.Surveys;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    public class TaskSurveyWait : BaseSensorTask
    {
        readonly MemStruct _MemInfo = new MemStruct(0x14);
        readonly MemVarUInt16 page = new MemVarUInt16(0x14);
        readonly MemVarUInt16 kolstr = new MemVarUInt16(0x16);
        readonly MemVarUInt16 kolbl = new MemVarUInt16(0x18);

        readonly MemStruct _CurrInfo = new MemStruct(0x8400);
        readonly MemVarFloat Dav = new MemVarFloat();
        readonly MemVarFloat Temp = new MemVarFloat();
        readonly MemVarFloat ExTemp = new MemVarFloat();
        readonly MemVarFloat Acc = new MemVarFloat();
        readonly MemVarFloat RaznR = new MemVarFloat();
        readonly MemVarFloat ObshR = new MemVarFloat();
        readonly MemVarUInt16 Emak = new MemVarUInt16();
        readonly MemVarUInt16 Emem = new MemVarUInt16();

        readonly MemVarUInt16 Kolisl = new MemVarUInt16();
        readonly MemVarUInt16 Schstr = new MemVarUInt16();
        readonly MemVarUInt16 Schbl = new MemVarUInt16();
        readonly MemVarUInt16 Koliz = new MemVarUInt16();

        readonly MemVarUInt8 OpReg = new MemVarUInt8(0x8800);
        readonly MemVarUInt16 StatusReg = new MemVarUInt16(0x8802);

        readonly MemStruct _SurvayParam1 = new MemStruct(0x8000);
        readonly MemVarUInt8 Vissl = new MemVarUInt8(0x8000);
        readonly MemStruct _SurvayParam2 = new MemStruct(0x8016);
        readonly MemVarUInt32 Interval = new MemVarUInt32(0x8016);
        readonly MemVarUInt16 Revbit = new MemVarUInt16(0x801A);



        Kind SurveyKind => (Kind)Vissl.Value;


        TimeSpan _Remain;
        TimeSpan _Total;
        public TaskSurveyWait(SensorModel sensor)
            : base(sensor, Resource.Survey)
        {
            _MemInfo.Add(page);
            _MemInfo.Add(kolstr);
            _MemInfo.Add(kolbl);

            _CurrInfo.Add(Dav);
            _CurrInfo.Add(Temp);
            _CurrInfo.Add(ExTemp);
            _CurrInfo.Add(Acc);
            _CurrInfo.Add(RaznR);
            _CurrInfo.Add(ObshR);
            _CurrInfo.Add(Emak);
            _CurrInfo.Add(Emem);

            _SurvayParam1.Add(Vissl);

            _SurvayParam2.Add(Interval);
            _SurvayParam2.Add(Revbit);
        }
        public override async Task DoBeforeCancelAsync()
        {
            OpReg.Value = 4;
            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
                await Connection.WriteAsync(OpReg, null, ctSrc.Token);
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;

            if (null == Connection)
                return false;
            InfoEx = "чтение статуса";
            await Connection.ReadAsync(StatusReg, null, ct);
            InfoEx = "чтение вида исследования";
            await Connection.ReadAsync(_SurvayParam1, null, ct);
            Name = $"{Resource.Survey} {SurveyKind.Title()}";
            InfoEx = "чтение";
            if (0 == StatusReg.Value)
                return true;

            InfoEx = "чтение информации о памяти";
            await Connection.ReadAsync(_MemInfo, null, ct);
            InfoEx = "чтение информации о измерении";
            await Connection.ReadAsync(_SurvayParam2, null, ct);
            InfoEx = "чтение текущей информации ";
            await Connection.ReadAsync(_CurrInfo, null, ct);
            ulong totalMem = (ulong)(kolbl.Value) * kolstr.Value * page.Value;

            ulong surSize = sizeof(UInt32);
            if (0 < (Revbit.Value & (1 << 1)))
                surSize += sizeof(UInt32);
            if (0 < (Revbit.Value & (4 << 1)))
                surSize += sizeof(UInt32);

            ulong totalSec = (Interval.Value / 10000) * totalMem;

            _Total = TimeSpan.FromSeconds(totalSec);
            _Remain = TimeSpan.FromSeconds(totalSec * (Emem.Value * 0.1f));

            double progressStart = (double)(1.0 - _Remain.TotalMilliseconds / _Total.TotalMilliseconds);

            //using (var timer = CreateProgressTimer(_Remain, (float)progressStart))
            {
                await DoMultiPressureAsync(ct);
            }
            return true;
        }
        async Task DoMultiPressureAsync(CancellationToken ct)
        {

            while (true && !ct.IsCancellationRequested)
            {
                InfoEx = $"Свободно памяти ~{Emem.Value * 0.1f}% \n измерение...";
                await Task.Delay(Constants.SecondDelay * 10, ct);
                InfoEx = "чтение текущей информации ";
                await Connection.ReadAsync(_CurrInfo, null, ct);
                Progress = 1.0f - Emem.Value * 0.1f;
            }
        }

    }
}
