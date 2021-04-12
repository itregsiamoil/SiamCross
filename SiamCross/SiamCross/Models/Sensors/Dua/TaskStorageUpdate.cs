using SiamCross.Models.Connection.Protocol;
using System;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class TaskStorageUpdate : BaseSensorTask
    {
        readonly DuaStorage _Storage;
        readonly MemVarUInt16 Uksh = new MemVarUInt16();
        readonly MemVarUInt16 Ukex = new MemVarUInt16();
        readonly MemStruct Aviable = new MemStruct(0x8418);

        public TaskStorageUpdate(DuaStorage model, ISensor sensor)
            : base(sensor, "Опрос хранилища")
        {
            if (model is DuaStorage storage)
                _Storage = storage;

            Aviable.Add(Uksh);
            Aviable.Add(Ukex);
        }

        public override async Task<bool> DoExecute()
        {
            if (null == _Storage || null == Connection || null == Sensor)
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
                ret = await Connection.ReadAsync(Aviable, null, _Cts.Token);
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
                _Storage.AviableRep = Uksh.Value;
                _Storage.AviableEcho = Ukex.Value;
                InfoEx = "успешно выполнено";
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
