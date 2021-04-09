using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Sensors.Dua.Surveys;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskSurveyLevel : BaseSensorTask
    {
        readonly Level _Model;

        //public readonly MemStruct Operating = new MemStruct(0x8800);
        public readonly MemVarUInt8 OpReg = new MemVarUInt8(null, 0x8800);
        public readonly MemVarUInt16 StatusReg = new MemVarUInt16(null, 0x8802);

        public readonly MemVarUInt16 Revbit = new MemVarUInt16(null, 0x8008);
        public readonly MemVarUInt8 Vissl = new MemVarUInt8(null, 0x800A);
        public readonly MemVarUInt16 Vzvuk = new MemVarUInt16(null, 0x801C);
        public readonly MemVarUInt16 Ntpop = new MemVarUInt16(null, 0x801E);

        public readonly MemVarUInt16 Timeawt = new MemVarUInt16(null, 0x8416);


        readonly static int _TimeoutValvePrepare = 120000;
        readonly static int _TimeoutSurvay3000 = 20000;
        readonly static int _TimeoutSurvay6000 = 2* _TimeoutSurvay3000;
        readonly static int _ReadTime = 40000;
        int _SurveyTime = 0;

        public TaskSurveyLevel(Level model, ISensor sensor,string name, byte levelMode)
            : base(sensor, name)
        {
            _Model = model;
            Vissl.Value = levelMode;
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
            BitVector32 myBV = new BitVector32(Revbit.Value);
            int bit0 = BitVector32.CreateMask();
            int bit1 = BitVector32.CreateMask(bit0);
            int bit2 = BitVector32.CreateMask(bit1);
            int bit3 = BitVector32.CreateMask(bit2);
            int bit4 = BitVector32.CreateMask(bit3);
            int bit5 = BitVector32.CreateMask(bit4);
            int bit6 = BitVector32.CreateMask(bit5);
            int bit7 = BitVector32.CreateMask(bit6);
            int bit8 = BitVector32.CreateMask(bit7);
            int bit9 = BitVector32.CreateMask(bit8);

            myBV[bit1] = _Model.IsValveAutomaticEnabled;
            myBV[bit2] = _Model.IsValveDurationShort;
            myBV[bit0] = _Model.IsValveDirectionInput;
            myBV[bit6] = _Model.IsPiezoDepthMax;
            myBV[bit9] = _Model.IsPiezoAdditionalGain;

            Revbit.Value = (UInt16)myBV.Data;
            Vzvuk.Value = (UInt16)(_Model.SoundSpeedFixed * 10);
            Ntpop.Value = _Model.SoundSpeedTableId;
            OpReg.Value = 1;

            //await DoBeforeCancelAsync();

            InfoEx = "запись параметров";
            await Connection.WriteAsync(Revbit, null, _Cts.Token);
            await Connection.WriteAsync(Vissl, null, _Cts.Token);
            await Connection.WriteAsync(Vzvuk, null, _Cts.Token);
            await Connection.WriteAsync(Ntpop, null, _Cts.Token);
            InfoEx = "запуск исследования";
            await Connection.WriteAsync(OpReg, null, _Cts.Token);
            await ProcessSurvey();
            return true;
        }

        private async Task ProcessSurvey()
        {
            Timeawt.Value = 120;
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            for (uint i = 0; i < _SurveyTime && DuMeasurementStatus.Сompleted!= status; i+= Constants.SecondDelay)
            {
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
            catch (Exception ex)
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
            catch (Exception ex)
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

