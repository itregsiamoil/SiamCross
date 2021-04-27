using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    class TaskStorageUpdate : BaseSensorTask
    {
        readonly Storage _Storage;
        readonly MemStruct _MemInfo = new MemStruct(0x14);
        readonly MemVarUInt16 page = new MemVarUInt16(0x14);
        readonly MemVarUInt16 kolstr = new MemVarUInt16(0x16);
        readonly MemVarUInt16 kolbl = new MemVarUInt16(0x18);

        readonly MemStruct _CurrInfo = new MemStruct(0x841A);
        readonly MemVarUInt16 Emem = new MemVarUInt16();
        readonly MemVarUInt16 Kolisl = new MemVarUInt16();
        readonly MemVarUInt16 Schstr = new MemVarUInt16();
        readonly MemVarUInt16 Schbl = new MemVarUInt16();
        readonly MemVarUInt16 Koliz = new MemVarUInt16();

        public TaskStorageUpdate(Storage model, SensorModel sensor)
           : base(sensor, "Опрос хранилища")
        {
            if (model is Storage storage)
                _Storage = storage;

            _MemInfo.Add(page);
            _MemInfo.Add(kolstr);
            _MemInfo.Add(kolbl);

            _CurrInfo.Add(Emem);
            _CurrInfo.Add(Kolisl);
            _CurrInfo.Add(Schstr);
            _CurrInfo.Add(Schbl);
            _CurrInfo.Add(Koliz);

        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Storage || null == Connection)
                return false;

            //_BytesProgress = 0;
            //_BytesTotal = Aviable.Size;

            using (var ctSrc = new CancellationTokenSource(TimeSpan.FromMilliseconds(Constants.ConnectTimeout)))
            {
                using (var linkTsc = CancellationTokenSource.CreateLinkedTokenSource(ctSrc.Token, ct))
                {
                    return await UpdateAsync(linkTsc.Token);
                }
            }
        }
        async Task<bool> UpdateAsync(CancellationToken ct)
        {
            if (null == _Storage || null == Connection)
                return false;
            if (!await CheckConnectionAsync(ct))
                return false;

            bool ret = false;
            InfoEx = "чтение информации";
            ret = RespResult.NormalPkg == await Connection.TryReadAsync(_MemInfo, null, ct);

            if (!ret)
                return false;
            InfoEx = "чтение ";
            ret = RespResult.NormalPkg == await Connection.TryReadAsync(_CurrInfo, null, ct);

            _Storage.TotalSpace = (ulong)(kolbl.Value) * kolstr.Value * page.Value;
            _Storage.EmptySpaceRatio = Math.Round(0.1f * Emem.Value, 1);
            _Storage.SurveyQty = Kolisl.Value;
            _Storage.CountRep = Kolisl.Value;

            return ret;
        }

    }
}
