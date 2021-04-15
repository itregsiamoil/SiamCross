using SiamCross.Models.Connection.Protocol;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskLoadSurveyInfo : BaseSensorTask
    {
        readonly DuaSurveyCfg _Model;

        public readonly MemStruct SurvayParam = new MemStruct(0x8008);
        public readonly MemVarUInt16 Revbit = new MemVarUInt16();
        public readonly MemVarUInt8 Vissl = new MemVarUInt8();
        public readonly MemVarByteArray Kust = new MemVarByteArray(0, new MemValueByteArray(5));
        public readonly MemVarByteArray Skv = new MemVarByteArray(0, new MemValueByteArray(6));
        public readonly MemVarUInt16 Field = new MemVarUInt16();
        public readonly MemVarUInt16 Shop = new MemVarUInt16();
        public readonly MemVarUInt16 Operator = new MemVarUInt16();
        public readonly MemVarUInt16 Vzvuk = new MemVarUInt16();
        public readonly MemVarUInt16 Ntpop = new MemVarUInt16();
        public readonly MemVarUInt8 PerP = new MemVarUInt8();
        public readonly MemVarUInt8 KolP = new MemVarUInt8();
        public readonly MemVarByteArray PerU = new MemVarByteArray(0, new MemValueByteArray(5));
        public readonly MemVarByteArray KolUr = new MemVarByteArray(0, new MemValueByteArray(5));

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }

        public TaskLoadSurveyInfo(DuaSurveyCfg model, ISensor sensor)
            : base(sensor, "Опрос параметров измерения")
        {
            _Model = model;
            SurvayParam.Add(Revbit);
            SurvayParam.Add(Vissl);
            SurvayParam.Add(Kust);
            SurvayParam.Add(Skv);
            SurvayParam.Add(Field);
            SurvayParam.Add(Shop);
            SurvayParam.Add(Operator);
            SurvayParam.Add(Vzvuk);
            SurvayParam.Add(Ntpop);
            SurvayParam.Add(PerP);
            SurvayParam.Add(KolP);
            SurvayParam.Add(PerU);
            SurvayParam.Add(KolUr);
        }

        public override async Task<bool> DoExecute()
        {
            if (null == _Model || null == Connection || null == Sensor)
                return false;

            Progress = 0;
            _BytesProgress = 0;
            _BytesTotal = SurvayParam.Size;
            _Cts.CancelAfter(20000);
            return await Update();
        }

        async Task<bool> SingleUpdate()
        {
            RespResult ret = await Connection.TryReadAsync(SurvayParam, SetProgressBytes, _Cts.Token);
            return RespResult.NormalPkg == ret;
        }

        public async Task<bool> Update()
        {
            bool readed = false;

            if (!_Model.Synched)
            {
                if (await CheckConnectionAsync())
                {
                    InfoEx = "чтение";
                    readed = await SingleUpdate();
                }
            }
            else
                InfoEx = "обновлено";

            if (readed)
            {
                //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _Model.IsAutoswitchToAPR = 0 < (Revbit.Value & (1 << 5));
                _Model.IsValveAutomaticEnabled = 0 < (Revbit.Value & (1 << 1));
                _Model.IsValveDurationShort = 0 < (Revbit.Value & (1 << 2));
                _Model.IsValveDirectionInput = 0 < (Revbit.Value & (1 << 0));
                _Model.IsPiezoDepthMax = 0 < (Revbit.Value & (1 << 6));
                _Model.IsPiezoAdditionalGain = 0 < (Revbit.Value & (1 << 9));
                _Model.SoundSpeedFixed = 0.1d * Vzvuk.Value;
                _Model.SoundSpeedTableId = Ntpop.Value;

                _Model.PressurePeriodIndex = PerP.Value;
                _Model.PressureQuantityIndex = KolP.Value;

                PerU.Value.CopyTo(_Model.LevelPeriodIndex, 0);
                KolUr.Value.CopyTo(_Model.LevelQuantityIndex, 0);

                InfoEx = "успешно выполнено";
                _Model.Synched = readed;
            }
            else if (!_Model.Synched)
            {
                _Model.IsAutoswitchToAPR = default;
                _Model.IsValveAutomaticEnabled = default;
                _Model.IsValveDurationShort = default;
                _Model.IsValveDirectionInput = default;
                _Model.IsPiezoDepthMax = default;
                _Model.IsPiezoAdditionalGain = default;
                _Model.SoundSpeedFixed = Constants.DefaultSoundSpeedFixed;
                _Model.SoundSpeedTableId = default;
                _Model.PressurePeriodIndex = default;
                _Model.PressureQuantityIndex = default;
                _Model.LevelPeriodIndex.ForEach((item) => item = 0);
                _Model.LevelQuantityIndex.ForEach((item) => item = 0);
                InfoEx = "set default";
            }

            _Model.ChangeNotify(nameof(_Model.IsAutoswitchToAPR));
            _Model.ChangeNotify(nameof(_Model.IsValveAutomaticEnabled));
            _Model.ChangeNotify(nameof(_Model.IsValveDurationShort));
            _Model.ChangeNotify(nameof(_Model.IsValveDirectionInput));
            _Model.ChangeNotify(nameof(_Model.IsPiezoDepthMax));
            _Model.ChangeNotify(nameof(_Model.IsPiezoAdditionalGain));
            _Model.ChangeNotify(nameof(_Model.SoundSpeedFixed));
            _Model.ChangeNotify(nameof(_Model.SoundSpeedTableId));
            _Model.ChangeNotify(nameof(_Model.PressurePeriodIndex));
            _Model.ChangeNotify(nameof(_Model.PressureQuantityIndex));
            for (uint i = 0; i < _Model.LevelPeriodIndex.Length; ++i)
            {
                _Model.ChangeNotify($"LevelPeriodIndex{i}");
                _Model.ChangeNotify($"LevelQuantityIndex{i}");
            }
            return _Model.Synched;
        }

    }
}
