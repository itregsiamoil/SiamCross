using SiamCross.Models.Connection.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    class TaskStorageRead : BaseSensorTask
    {
        readonly Storage _Storage;

        readonly MemStruct _CurrMemInfo = new MemStruct(0x0E);
        readonly MemVarUInt16 Adrtek = new MemVarUInt16(0x0E);
        readonly MemVarUInt16 Ukstr = new MemVarUInt16(0x10);
        readonly MemVarUInt16 Ukbl = new MemVarUInt16(0x12);

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

        readonly MemStruct _Rep = new MemStruct(0x00);
        readonly MemVarUInt16 KSum = new MemVarUInt16();
        readonly MemVarUInt16 KAdrPr = new MemVarUInt16();
        readonly MemVarUInt16 KolToch = new MemVarUInt16();
        readonly MemVarUInt16 KolStrt = new MemVarUInt16();
        readonly MemVarUInt16 KolBlt = new MemVarUInt16();
        readonly MemVarUInt16 NomIslt = new MemVarUInt16();
        readonly MemVarUInt8 KolPar = new MemVarUInt8();
        readonly MemVarUInt8 VisslRep = new MemVarUInt8();
        readonly MemVarByteArray KustRep = new MemVarByteArray(0, new MemValueByteArray(5));
        readonly MemVarByteArray SkvRep = new MemVarByteArray(0, new MemValueByteArray(6));
        readonly MemVarUInt16 FieldRep = new MemVarUInt16();
        readonly MemVarUInt16 ShopRep = new MemVarUInt16();
        readonly MemVarUInt16 OperatorRep = new MemVarUInt16();
        readonly MemVarByteArray StartTimestamp = new MemVarByteArray(0, new MemValueByteArray(6));
        readonly MemVarUInt32 IntervalRep = new MemVarUInt32();
        //readonly MemVarFloat DavRep = new MemVarFloat();
        //readonly MemVarFloat TempRep = new MemVarFloat();
        //readonly MemVarFloat ExTempRep = new MemVarFloat();


        public TaskStorageRead(Storage model, SensorModel sensor)
            : base(sensor, "Чтение исследований")
        {
            if (model is Storage storage)
                _Storage = storage;

            _CurrMemInfo.Add(Adrtek);
            _CurrMemInfo.Add(Ukstr);
            _CurrMemInfo.Add(Ukbl);

            _MemInfo.Add(page);
            _MemInfo.Add(kolstr);
            _MemInfo.Add(kolbl);

            _CurrInfo.Add(Emem);
            _CurrInfo.Add(Kolisl);
            _CurrInfo.Add(Schstr);
            _CurrInfo.Add(Schbl);
            _CurrInfo.Add(Koliz);

            _Rep.Add(KSum);
            _Rep.Add(KAdrPr);
            _Rep.Add(KolToch);
            _Rep.Add(KolStrt);
            _Rep.Add(KolBlt);
            _Rep.Add(NomIslt);
            _Rep.Add(KolPar);
            _Rep.Add(VisslRep);
            _Rep.Add(KustRep);
            _Rep.Add(SkvRep);
            _Rep.Add(FieldRep);
            _Rep.Add(ShopRep);
            _Rep.Add(OperatorRep);
            _Rep.Add(StartTimestamp);
            _Rep.Add(IntervalRep);

        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Storage || null == Connection)
                return false;
            if (!await CheckConnectionAsync(ct))
                return false;
            InfoEx = "чтение информации о памяти";
            await Connection.ReadAsync(_MemInfo, null, ct);
            InfoEx = "чтение текущего состояния памяти";
            await Connection.ReadAsync(_CurrMemInfo, null, ct);
            InfoEx = "чтение количества исследованиу";
            await Connection.ReadAsync(_CurrInfo, null, ct);


            if (0xFFFF==Adrtek.Value || 0==Kolisl.Value)
            {
                InfoEx = "пусто";
                return true;
            }
            
            if(0< (Adrtek.Value & (15<<1)) )
            {
                await ReadHeaderFlash(ct);
            }
            else 
            {
                await ReadHeaderFram(ct);
            }
            

            return true;
        }
        async Task ReadHeaderFram(CancellationToken ct)
        {
            InfoEx = "чтение текущего заголовка из FRAM";
            _Rep.Address = 0x70000000 + (uint)(Adrtek.Value & 0x7FFF);
            await Connection.ReadAsync(_Rep, null, ct);

        }
        async Task ReadHeaderFlash(CancellationToken ct)
        {
            InfoEx = "чтение текущего заголовка из FLASH";
            _Rep.Address = 0x80000000 + (uint)(Adrtek.Value & 0x7FFF);
            await Connection.ReadAsync(_Rep, null, ct);
        }
    }
}
