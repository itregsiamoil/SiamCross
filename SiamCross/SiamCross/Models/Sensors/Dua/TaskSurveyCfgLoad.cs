using SiamCross.Models.Connection.Protocol;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskSurveyCfgLoad : BaseSensorTask
    {
        readonly DuaSurveyCfg _Model;

        readonly MemStruct SurvayParam = new MemStruct(0x8008);
        readonly MemVarUInt16 Revbit = new MemVarUInt16();
        readonly MemVarUInt8 Vissl = new MemVarUInt8();
        readonly MemVarByteArray Kust = new MemVarByteArray(0, new MemValueByteArray(5));
        readonly MemVarByteArray Skv = new MemVarByteArray(0, new MemValueByteArray(6));
        readonly MemVarUInt16 Field = new MemVarUInt16();
        readonly MemVarUInt16 Shop = new MemVarUInt16();
        readonly MemVarUInt16 Operator = new MemVarUInt16();
        readonly MemVarUInt16 Vzvuk = new MemVarUInt16();
        readonly MemVarUInt16 Ntpop = new MemVarUInt16();
        readonly MemVarUInt8 PerP = new MemVarUInt8();
        readonly MemVarUInt8 KolP = new MemVarUInt8();
        readonly MemVarByteArray PerU = new MemVarByteArray(0, new MemValueByteArray(5));
        readonly MemVarByteArray KolUr = new MemVarByteArray(0, new MemValueByteArray(5));

        readonly MemStruct CurrParam = new MemStruct(0x8406);
        readonly MemVarByteArray Timestamp = new MemVarByteArray(0x8406, new MemValueByteArray(6));

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }
        public TaskSurveyCfgLoad(DuaSurveyCfg model, SensorModel sensor)
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

            CurrParam.Add(Timestamp);
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Model || null == Connection)
                return false;

            _BytesProgress = 0;
            _BytesTotal = SurvayParam.Size + CurrParam.Size;

            using (var ctSrc = new CancellationTokenSource(Constants.ConnectTimeout))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await UpdateAsync(linkTsc.Token);
                }
            }
        }


        async Task LoadTimestamp(CancellationToken ct)
        {
            RespResult res = RespResult.ErrorUnknown;
            InfoEx = "чтение времени";
            res = await Connection.TryReadAsync(Timestamp, SetProgressBytes, ct);

            if (RespResult.NormalPkg == res)
            {
                var dt = DateTime.Now;
                var epoh = dt.Year - dt.Year % 100;
                var year = (100 > Timestamp.Value[5]) ? epoh + Timestamp.Value[5] : Timestamp.Value[5];

                try
                {
                    _Model.Timestamp = new DateTime(
                        year, Timestamp.Value[4], Timestamp.Value[3]
                        , Timestamp.Value[0], Timestamp.Value[1], Timestamp.Value[2]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: invalid date {ex.Message}");
                    _Model.Timestamp = DateTime.MinValue;
                }
            }
            else
                _Model.Timestamp = DateTime.MinValue;
            _Model.ChangeNotify(nameof(_Model.Timestamp));
        }

        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;
            await LoadTimestamp(ct);

            bool readed = false;
            if (!_Model.Synched)
            {
                InfoEx = "чтение параметров";
                readed = RespResult.NormalPkg == await Connection.TryReadAsync(SurvayParam, SetProgressBytes, ct);
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

                InfoEx = "выполнено";
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
            _Model.ChangeNotify(nameof(_Model.Timestamp));
            for (uint i = 0; i < _Model.LevelPeriodIndex.Length; ++i)
            {
                _Model.ChangeNotify($"LevelPeriodIndex{i}");
                _Model.ChangeNotify($"LevelQuantityIndex{i}");
            }
            return _Model.Synched;
        }

    }
}
