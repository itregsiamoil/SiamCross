using SiamCross.Models.Connection.Protocol;
using SiamCross.Services;
using SiamCross.Services.Environment;
using System;
using System.IO;
using System.Text;
using System.Threading;
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

        public TaskStorageRead(DuaStorage model, SensorModel sensor)
            : base(sensor, "Чтение исследований")
        {
            Sensor = sensor;

            if (model is DuaStorage storage)
                _Storage = storage;

            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            ТempC = _CurrentParam.Add(new MemVarUInt16(nameof(ТempC)));
            Pressure = _CurrentParam.Add(new MemVarInt16(nameof(Pressure)));
            Time = _CurrentParam.Add(new MemVarByteArray(0, new MemValueByteArray(8), nameof(Time)));
            Emak = _CurrentParam.Add(new MemVarUInt16(nameof(Emak)));
            Rdav = _CurrentParam.Add(new MemVarUInt16(nameof(Rdav)));
            TimeRequre = _CurrentParam.Add(new MemVarByteArray(0, new MemValueByteArray(3), nameof(TimeRequre)));
            Interv = _CurrentParam.Add(new MemVarUInt8(nameof(Interv)));
            Kolt = _CurrentParam.Add(new MemVarUInt16(nameof(Kolt)));
            Timeawt = _CurrentParam.Add(new MemVarUInt16(nameof(Timeawt)));
            Uksh = _CurrentParam.Add(new MemVarUInt16(nameof(Uksh)));
            Ukex = _CurrentParam.Add(new MemVarUInt16(nameof(Ukex)));

            ReportHeader = new MemStruct(0x82000000);
            vissl = ReportHeader.Add(new MemVarUInt8());
            kust = ReportHeader.Add(new MemVarByteArray(0, new MemValueByteArray(5)));
            skv = ReportHeader.Add(new MemVarByteArray(0, new MemValueByteArray(6)));
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

            Echo = new MemVarByteArray(0, new MemValueByteArray(_EchoSize));
        }

        uint _BytesTotal;
        uint _BytesReaded;

        void SetProgressBytes(uint bytes)
        {
            _BytesReaded += bytes;
            Progress = ((float)_BytesReaded / _BytesTotal);
        }

        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Storage || null == Connection)
                return false;

            uint bytesEcho = _Storage.CountEcho * (ReportHeader.Size + _EchoSize);
            uint bytesSym = _Storage.CountRep * (ReportHeader.Size);
            _BytesTotal = bytesEcho + bytesSym;
            _BytesReaded = 0;

            await DoReadEchoAsync(true, _Storage.StartEcho, _Storage.CountEcho, ct);
            if (ct.IsCancellationRequested)
                return false;
            await DoReadEchoAsync(false, _Storage.StartRep, _Storage.CountRep, ct);
            if (ct.IsCancellationRequested)
                return false;

            return true;
        }

        protected async Task<bool> DoReadEchoAsync(bool echo, uint begin, uint qty, CancellationToken ct)
        {
            if (!await CheckConnectionAsync(ct))
                return false;

            uint reportBaseAddress = echo ? 0x83000000 : 0x82000000;

            for (UInt32 rec = 0; rec < qty; ++rec)
            {
                InfoEx = $"чтение {rec + 1} измерения " + (echo ? "с эхограммой" : string.Empty);

                ReportHeader.Address = reportBaseAddress + ReportHeader.Size * (begin + rec);
                await Connection.ReadAsync(ReportHeader, null, ct);
                SetProgressBytes(ReportHeader.Size);

                if (echo)
                {
                    var tmp = Connection.AdditioonalTimeout;
                    Connection.AdditioonalTimeout = 9000;
                    Echo.Address = 0x84000000 + _EchoSize * rec;
                    await Connection.ReadMemAsync(Echo.Address, Echo.Size, Echo.Value, 0, SetProgressBytes, ct);
                    Connection.AdditioonalTimeout = tmp;
                }

                var well = Encoding.UTF8.GetString(skv.Value);
                var bush = Encoding.UTF8.GetString(kust.Value);
                var pos = new Position(field.Value, well, bush, shop.Value);
                var mi = new MeasurementInfo()
                {
                    Kind = 1,
                    BeginTimestamp = GetTimestamp(),
                    EndTimestamp = GetTimestamp(),
                    Comment = string.Empty,
                };

                mi.Set("sudresearchtype", (long)vissl.Value);
                mi.Set("lgsoundspeed", (double)vzvuk.Value / 10.0);
                mi.Set("sudcorrectiontype", (long)ntpop.Value);
                mi.Set("lgreflectioncount", (long)kolotr.Value);
                mi.Set("lglevel", (double)urov.Value);
                mi.Set("sudpressure", (double)pressure.Value / 10.0);
                mi.Set("lgtimediscrete", (double)0.00585938);

                if (echo)
                {
                    var echoStream = EnvironmentService.CreateTempFileSurvey();
                    if (null != echoStream)
                    {
                        using (echoStream)
                        {
                            await echoStream.WriteAsync(Echo.Value, 0, (int)Echo.Size, ct);
                            mi.SetBlob("lgechogram", Path.GetFileName(echoStream.Name));
                            echoStream.Close();
                        }
                    }
                }

                MeasureData survey = new MeasureData(
                     pos
                    , Sensor.Device
                    , Sensor.Info
                    , mi);
                await DbService.Instance.SaveSurveyAsync(survey);
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
