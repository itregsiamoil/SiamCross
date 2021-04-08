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
    public class TaskSurveyStaticLevel : BaseSensorTask
    {
        readonly StaticLevel _Model;

        //public readonly MemStruct Operating = new MemStruct(0x8800);
        public readonly MemVarUInt8 OpReg = new MemVarUInt8(null, 0x8800);
        public readonly MemVarUInt16 StatusReg = new MemVarUInt16(null, 0x8802);

        public readonly MemVarUInt16 Revbit = new MemVarUInt16(null, 0x8008);
        public readonly MemVarUInt8 Vissl = new MemVarUInt8(null, 0x800A);
        public readonly MemVarUInt16 Vzvuk = new MemVarUInt16(null, 0x801C);
        public readonly MemVarUInt16 Ntpop = new MemVarUInt16(null, 0x801E);

        public readonly MemVarUInt16 Timeawt = new MemVarUInt16(null, 0x8416);

        readonly static uint _SurveyTime=80000;
        readonly static uint _ReadTime = 40000;

        public TaskSurveyStaticLevel(StaticLevel model, ISensor sensor)
            : base(sensor, "исследование статический уровень")
        {
            _Model = model;
        }

        public override async Task<bool> DoExecute()
        {
            if (null == _Model || null == Connection || null == Sensor)
                return false;
            using (var timer = CreateProgressTimer(_SurveyTime+ _ReadTime))
                return await Update();
        }

        async Task<bool> SingleUpdate()
        {
            RespResult ret = RespResult.ErrorUnknown;
            try
            {
                //ret = await Connection.ReadAsync(Revbit, null, _Cts.Token);

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
                Vissl.Value = 1;
                Vzvuk.Value = (UInt16)(_Model.SoundSpeedFixed*10);
                Ntpop.Value = _Model.SoundSpeedTableId;
                OpReg.Value = 1;

                InfoEx = "запись параметров";
                ret = await Connection.WriteAsync(Revbit, null, _Cts.Token);
                ret = await Connection.WriteAsync(Vissl, null, _Cts.Token);
                ret = await Connection.WriteAsync(Vzvuk, null, _Cts.Token);
                ret = await Connection.WriteAsync(Ntpop, null, _Cts.Token);
                ret = await Connection.WriteAsync(OpReg, null, _Cts.Token);

                await ProcessSurvey();
            }
            catch (Exception ex)
            {
                ret = RespResult.ErrorUnknown;
                Debug.WriteLine("EXCEPTION in "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n type=" + ex.GetType() + ": msg=" + ex.Message
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return RespResult.NormalPkg == ret;
        }

        public async Task<bool> Update()
        {
            bool ret = false;
            if (await CheckConnectionAsync())
            {
                InfoEx = "запуск";
                ret = await SingleUpdate();
            }
            return ret;
        }

        private async Task<DuMeasurementStatus> GetStatus()
        {
            await Connection.ReadAsync(StatusReg, null, _Cts.Token);
            return (DuMeasurementStatus)StatusReg.Value;
        }
        private async Task ProcessSurvey()
        {
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            bool isDone = false;
            UInt32 measure_time_sec = _SurveyTime / 1000;//18/36
            for (UInt32 i = 0; i < measure_time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay, _Cts.Token);
                status = await GetStatus();
                Debug.WriteLine(DuStatusAdapter.StatusToString(status));

                switch (status)
                {
                    case DuMeasurementStatus.Сompleted:
                        isDone = true;
                        break;
                    default:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.WaitingForClick:
                    case DuMeasurementStatus.Empty:
                    case DuMeasurementStatus.NoiseMeasurement:
                        InfoEx = DuStatusAdapter.StatusToString(status);
                        break;
                    case DuMeasurementStatus.ValvePreparation:
                        //await Connection.ReadAsync(Timeawt, null, _Cts.Token);
                        InfoEx = DuStatusAdapter.StatusToString(status) +$", осталось {Timeawt.Value}сек.";
                        break;
                }
            }
        }

        public override Task DoBeforeCancelAsync()
        {
            OpReg.Value = 4;
            return Connection.WriteAsync(OpReg, null, _Cts.Token);
        }

    }
}

