using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Sensors.Dua.Surveys;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskSurveyLevel : BaseSensorTask
    {
        readonly Level _Model;

        //public readonly MemStruct Operating = new MemStruct(0x8800);
        public readonly MemVarUInt8 OpReg = new MemVarUInt8(null, 0x8800);
        public readonly MemVarUInt16 StatusReg = new MemVarUInt16(null, 0x8802);
        public readonly MemVarUInt16 Timeawt = new MemVarUInt16(null, 0x8416);

        static readonly int _TimeoutValvePrepare = 120000;
        static readonly int _TimeoutSurvay3000 = 20000;
        static readonly int _TimeoutSurvay6000 = 2 * _TimeoutSurvay3000;

        int _SurveyTime = 0;

        public TaskSurveyLevel(Level model, ISensor sensor, string name)
            : base(sensor, name)
        {
            _Model = model;
        }

        public override async Task<bool> DoExecute()
        {
            if (null == _Model || null == Connection || null == Sensor)
                return false;

            _SurveyTime = _TimeoutValvePrepare + (_Model.IsPiezoDepthMax ? _TimeoutSurvay6000 : _TimeoutSurvay3000);
            _Cts.CancelAfter(_SurveyTime * 2);

            using (var timer = CreateProgressTimer(_SurveyTime))
                return await SingleUpdate();
        }

        async Task<bool> SingleUpdate()
        {
            if (!await CheckConnectionAsync())
                return false;
        
            OpReg.Value = 1;

            //await DoBeforeCancelAsync();

            InfoEx = "запуск исследования";
            await Connection.WriteAsync(OpReg, null, _Cts.Token);
            await ProcessSurvey();
            return true;
        }

        private async Task ProcessSurvey()
        {
            Timeawt.Value = 120;
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            for (uint i = 0; i < _SurveyTime && DuMeasurementStatus.Сompleted != status; i += Constants.SecondDelay)
            {
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
                        InfoEx = DuStatusAdapter.StatusToString(status);
                        break;
                    case DuMeasurementStatus.ValvePreparation:
                        await UpdateValvePreparation();
                        InfoEx = DuStatusAdapter.StatusToString(status) + $", осталось {Timeawt.Value}сек.";
                        break;
                }
            }
        }

        private async Task UpdateStatus()
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
        private async Task UpdateValvePreparation()
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
        public override Task DoBeforeCancelAsync()
        {
            OpReg.Value = 4;
            return Connection.WriteAsync(OpReg, null, _Cts.Token);
        }

    }
}

