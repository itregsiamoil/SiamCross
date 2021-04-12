using SiamCross.Models.Connection.Protocol;
using SiamCross.Services;
using System;
using System.Text;
using System.Threading.Tasks;


namespace SiamCross.Models.Sensors.Dua
{
    public class TaskStorageRead : BaseSensorTask
    {
        readonly DuaStorage _Storage;

        const uint _EchoSize = 3000;

        readonly MemStruct _CurrentParam;//0x8400
        readonly MemVarUInt16 BatteryVoltage;
        readonly MemVarUInt16 ТempC;
        readonly MemVarInt16 Pressure;
        readonly MemVarByteArray Time;
        readonly MemVarUInt16 Emak;
        readonly MemVarUInt16 Rdav;
        readonly MemVarByteArray TimeRequre;
        readonly MemVarUInt8 Interv;
        readonly MemVarUInt16 Kolt;
        readonly MemVarUInt16 Timeawt;
        readonly MemVarUInt16 Uksh;
        readonly MemVarUInt16 Ukex;

        readonly MemStruct ReportHeader;
        readonly MemVarUInt8 vissl;
        readonly MemVarByteArray kust;
        readonly MemVarByteArray skv;
        readonly MemVarUInt16 field;
        readonly MemVarUInt16 shop;
        readonly MemVarUInt16 user;
        readonly MemVarUInt16 vzvuk;
        readonly MemVarUInt16 ntpop;
        readonly MemVarUInt8 hour;
        readonly MemVarUInt8 min;
        readonly MemVarUInt8 sec;
        readonly MemVarUInt8 date;
        readonly MemVarUInt8 month;
        readonly MemVarUInt8 year;
        readonly MemVarUInt16 kolotr;
        readonly MemVarUInt16 urov;
        readonly MemVarInt16 pressure;

        readonly MemVarByteArray Echo;

        public TaskStorageRead(DuaStorage model, ISensor sensor)
            : base(sensor, "Опрос хранилища")
        {
            if (model is DuaStorage storage)
                _Storage = storage;

            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            ТempC = _CurrentParam.Add(new MemVarUInt16(nameof(ТempC)));
            Pressure = _CurrentParam.Add(new MemVarInt16(nameof(Pressure)));
            Time = _CurrentParam.Add(new MemVarByteArray(nameof(Time), 0, new MemValueByteArray(8)));
            Emak = _CurrentParam.Add(new MemVarUInt16(nameof(Emak)));
            Rdav = _CurrentParam.Add(new MemVarUInt16(nameof(Rdav)));
            TimeRequre = _CurrentParam.Add(new MemVarByteArray(nameof(TimeRequre), 0, new MemValueByteArray(3)));
            Interv = _CurrentParam.Add(new MemVarUInt8(nameof(Interv)));
            Kolt = _CurrentParam.Add(new MemVarUInt16(nameof(Kolt)));
            Timeawt = _CurrentParam.Add(new MemVarUInt16(nameof(Timeawt)));
            Uksh = _CurrentParam.Add(new MemVarUInt16(nameof(Uksh)));
            Ukex = _CurrentParam.Add(new MemVarUInt16(nameof(Ukex)));

            ReportHeader = new MemStruct(0x82000000);
            vissl = ReportHeader.Add(new MemVarUInt8());
            kust = ReportHeader.Add(new MemVarByteArray(null, 0, new MemValueByteArray(5)));
            skv = ReportHeader.Add(new MemVarByteArray(null, 0, new MemValueByteArray(6)));
            field = ReportHeader.Add(new MemVarUInt16());
            shop = ReportHeader.Add(new MemVarUInt16());
            user = ReportHeader.Add(new MemVarUInt16());
            vzvuk = ReportHeader.Add(new MemVarUInt16());
            ntpop = ReportHeader.Add(new MemVarUInt16());
            hour = ReportHeader.Add(new MemVarUInt8());
            min = ReportHeader.Add(new MemVarUInt8());
            sec = ReportHeader.Add(new MemVarUInt8());
            date = ReportHeader.Add(new MemVarUInt8());
            month = ReportHeader.Add(new MemVarUInt8());
            year = ReportHeader.Add(new MemVarUInt8());
            kolotr = ReportHeader.Add(new MemVarUInt16());
            urov = ReportHeader.Add(new MemVarUInt16());
            pressure = ReportHeader.Add(new MemVarInt16());

            Echo = new MemVarByteArray(null, 0, new MemValueByteArray(_EchoSize));
        }

        uint _BytesTotal;
        uint _BytesReaded;

        void SetProgressBytes(uint bytes)
        {
            _BytesReaded += bytes;
            Progress = ((float)_BytesReaded / _BytesTotal);
        }

        public override async Task<bool> DoExecute()
        {
            if (null == _Storage || null == Connection || null == Sensor)
                return false;

            uint bytesEcho = _Storage.CountEcho * (ReportHeader.Size + _EchoSize);
            uint bytesSym = _Storage.CountRep * (ReportHeader.Size);
            _BytesTotal = bytesEcho + bytesSym;
            _BytesReaded = 0;
            //uint timeoutTotal = bytesTotal * 2; // среднее время чтения 1 байта

            //using (var timer = CreateProgressTimer((int)timeoutTotal))
            {
                if (!await CheckConnectionAsync())
                    return false;
                await DoReadEcho();


            }
            return true;
        }

        void OnEchoStep(uint readed)
        {
            SetProgressBytes(readed);
        }

        protected async Task<bool> DoReadEcho()
        {
            var begin = _Storage.StartEcho;
            var qty = _Storage.CountEcho;


            for (UInt32 rec = 0; rec < qty; ++rec)
            {
                InfoEx = $"чтение {rec + 1} измерения с эхограммой";

                ReportHeader.Address = 0x82000000 + ReportHeader.Size * (begin + rec);
                await Connection.ReadAsync(ReportHeader, null, _Cts.Token);
                SetProgressBytes(ReportHeader.Size);

                Connection.AdditioonalTimeout = 9000;
                Echo.Address = 0x84000000 + _EchoSize * rec;

                await Connection.ReadMemAsync(Echo.Address, Echo.Size, Echo.Value, 0, OnEchoStep, _Cts.Token);


                var well = Encoding.UTF8.GetString(skv.Value);
                var bush = Encoding.UTF8.GetString(kust.Value);

                var pos = new PositionInfo(field.Value, well, bush, shop.Value);

                var mi = new MeasurementInfo()
                {
                    Kind = 1,
                    BeginTimestamp = GetTimestamp(),
                    EndTimestamp = GetTimestamp(),
                    Comment = string.Empty,
                };

                mi.DataInt.Add("sudresearchtype", vissl.Value);
                mi.DataFloat.Add("lgsoundspeed", vzvuk.Value / 10.0);
                mi.DataInt.Add("sudcorrectiontype", ntpop.Value);
                mi.DataInt.Add("lgreflectioncount", kolotr.Value);
                mi.DataFloat.Add("lglevel", urov.Value);
                mi.DataFloat.Add("sudpressure", pressure.Value / 10.0);
                mi.DataFloat.Add("lgtimediscrete", 0.00585938);
                mi.DataBlob.Add("lgechogram", Echo.Value);

                MeasureData survey = new MeasureData(
                     pos
                    , Sensor.ScannedDeviceInfo.Device
                    , Sensor.Info
                    , mi);
                await DbService.Instance.SaveMeasurement(survey);

            }


            return true;
        }

        DateTime GetTimestamp()
        {
            try
            {
                return new DateTime(year.Value, month.Value, date.Value
                            , hour.Value, min.Value, sec.Value);
            }
            catch (Exception)
            {

            }
            return DateTime.Now;
        }
    }
}
