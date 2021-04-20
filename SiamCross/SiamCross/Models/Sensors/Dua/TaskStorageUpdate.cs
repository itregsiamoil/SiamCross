using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskStorageUpdate : BaseSensorTask
    {
        readonly DuaStorage _Storage;
        readonly MemVarUInt16 Uksh = new MemVarUInt16();
        readonly MemVarUInt16 Ukex = new MemVarUInt16();
        readonly MemStruct Aviable = new MemStruct(0x8418);

        uint _BytesTotal;
        uint _BytesProgress;
        void SetProgressBytes(uint bytes)
        {
            _BytesProgress += bytes;
            Progress = ((float)_BytesProgress / _BytesTotal);
        }

        public TaskStorageUpdate(DuaStorage model, SensorModel sensor)
            : base(sensor, "Опрос хранилища")
        {
            if (model is DuaStorage storage)
                _Storage = storage;

            Aviable.Add(Uksh);
            Aviable.Add(Ukex);
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Storage || null == Connection)
                return false;

            _BytesProgress = 0;
            _BytesTotal = Aviable.Size;

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
            bool ret = false;
            if (await CheckConnectionAsync(ct))
            {
                InfoEx = "чтение";
                ret = RespResult.NormalPkg == await Connection.TryReadAsync(Aviable, SetProgressBytes, ct);
            }

            if (ret)
            {
                //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _Storage.AviableRep = Uksh.Value;
                _Storage.AviableEcho = Ukex.Value;
                InfoEx = "выполнено";
            }
            else
            {
                _Storage.AviableRep = 0;
                _Storage.AviableEcho = 0;
                InfoEx = "не выполнено";
            }

            _Storage.CountRep = _Storage.AviableRep;
            _Storage.CountEcho = _Storage.AviableEcho;

            _Storage.ChangeNotify(nameof(_Storage.AviableRep));
            _Storage.ChangeNotify(nameof(_Storage.AviableEcho));
            _Storage.ChangeNotify(nameof(_Storage.CountRep));
            _Storage.ChangeNotify(nameof(_Storage.CountEcho));


            return ret;
        }


    }
}
