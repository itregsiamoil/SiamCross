using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Dua.Surveys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskUpdateLevelInfo : BaseSensorTask
    {
        readonly Level _Model;

        public readonly MemStruct SurvayParam = new MemStruct(0x8000);
        public readonly MemVarUInt16 Chdav = new MemVarUInt16();
        public readonly MemVarUInt16 Chpiezo = new MemVarUInt16();
        public readonly MemVarUInt16 Noldav = new MemVarUInt16();
        public readonly MemVarUInt16 Nolexo = new MemVarUInt16();
        public readonly MemVarUInt16 Revbit = new MemVarUInt16();
        public readonly MemVarUInt8 Vissl = new MemVarUInt8();
        public readonly MemVarByteArray Kust = new MemVarByteArray(null, 0, new MemValueByteArray(5));
        public readonly MemVarByteArray Skv = new MemVarByteArray(null, 0, new MemValueByteArray(6));
        public readonly MemVarUInt16 Field = new MemVarUInt16();
        public readonly MemVarUInt16 Shop = new MemVarUInt16();
        public readonly MemVarUInt16 Operator = new MemVarUInt16();
        public readonly MemVarUInt16 Vzvuk = new MemVarUInt16();
        public readonly MemVarUInt16 Ntpop = new MemVarUInt16();

        public TaskUpdateLevelInfo(Level model, ISensor sensor)
            : base(sensor, "Опрос параметров измерения")
        {
            _Model = model;
            SurvayParam.Add(Chdav);
            SurvayParam.Add(Chpiezo);
            SurvayParam.Add(Noldav);
            SurvayParam.Add(Nolexo);
            SurvayParam.Add(Revbit);
            SurvayParam.Add(Vissl);
            SurvayParam.Add(Kust);
            SurvayParam.Add(Skv);
            SurvayParam.Add(Field);
            SurvayParam.Add(Shop);
            SurvayParam.Add(Operator);
            SurvayParam.Add(Vzvuk);
            SurvayParam.Add(Ntpop);
        }

        public override async Task<bool> DoExecute()
        {
            if (null == _Model || null == Connection || null == Sensor)
                return false;
            using (var timer = CreateProgressTimer(25000))
                return await Update();
        }

        async Task<bool> SingleUpdate()
        {
            RespResult ret = RespResult.ErrorUnknown;
            try
            {
                var tmp = Connection.AdditioonalTimeout;
                Connection.AdditioonalTimeout = 2000;
                ret = await Connection.ReadAsync(SurvayParam, null, _Cts.Token);
                Connection.AdditioonalTimeout = tmp;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return RespResult.NormalPkg == ret;
        }

        public async Task<bool> Update()
        {
            bool ret = false;
            if (await CheckConnectionAsync())
            {
                InfoEx = "чтение";
                ret = await RetryExecAsync(3, SingleUpdate);
            }

            if (ret)
            {
                //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _Model.IsValveAutomaticEnabled = 0 < (Revbit.Value & 1 << 1);
                _Model.IsValveDurationShort = 0 < (Revbit.Value & 1 << 2);
                _Model.IsValveDirectionInput = 0 < (Revbit.Value & 1 << 0);
                _Model.IsPiezoDepthMax = 0 < (Revbit.Value & 1 << 6);
                _Model.IsPiezoAdditionalGain = 0 < (Revbit.Value & 1 << 9);
                _Model.SoundSpeedFixed = 0.1d * Vzvuk.Value;
                _Model.SoundSpeedTableId = Ntpop.Value;
        
                InfoEx = "успешно выполнено";
            }
            else
            {
                _Model.IsValveAutomaticEnabled = default;
                _Model.IsValveDurationShort = default;
                _Model.IsValveDirectionInput = default;
                _Model.IsPiezoDepthMax = default;
                _Model.IsPiezoAdditionalGain = default;
                _Model.SoundSpeedFixed = Level.DefaultSoundSpeedFixed;
                _Model.SoundSpeedTableId = default;
            }

            _Model.ChangeNotify(nameof(_Model.IsValveAutomaticEnabled));
            _Model.ChangeNotify(nameof(_Model.IsValveDurationShort));
            _Model.ChangeNotify(nameof(_Model.IsValveDirectionInput));
            _Model.ChangeNotify(nameof(_Model.IsPiezoDepthMax));
            _Model.ChangeNotify(nameof(_Model.IsPiezoAdditionalGain));
            _Model.ChangeNotify(nameof(_Model.SoundSpeedFixed));
            _Model.ChangeNotify(nameof(_Model.SoundSpeedTableId));

            return ret;
        }

    }
}
