using SiamCross.Models.Connection.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg
{
    public class TaskSurveyWait : BaseSensorTask
    {
        readonly MemVarUInt16 CtrlReg = new MemVarUInt16(0x8800);
        readonly MemVarUInt16 StatReg = new MemVarUInt16(0x8802);


        TimeSpan _Remain;
        TimeSpan _Total;

        public TaskSurveyWait(SensorModel sensor)
            : base(sensor, Resource.Survey)
        {
            CtrlReg.Value = 0x02;
        }
        public override async Task DoBeforeCancelAsync()
        {
            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
                await Connection.WriteAsync(CtrlReg, null, ctSrc.Token);
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            _Total = TimeSpan.FromSeconds(180);
            _Remain = TimeSpan.FromSeconds(180);
            double progressStart = (double)(1.0 - _Remain.TotalMilliseconds / _Total.TotalMilliseconds);
            using (var timer = CreateProgressTimer(_Remain, (float)progressStart))
            {
                await DoWait(ct);
            }
            return true;
        }
        async Task DoWait(CancellationToken ct)
        {
            await Connection.ReadAsync(StatReg, null, ct);

            while (!ct.IsCancellationRequested
                && 0 != StatReg.Value
                && 4 != StatReg.Value 
                && 5 != StatReg.Value)
            {
                await Task.Delay(Constants.SecondDelay, ct);
                await Connection.ReadAsync(StatReg, null, ct);
                InfoEx = DmgMeasureStatusAdapter.StatusToReport(StatReg.Value);
            }
        }
    }
}
