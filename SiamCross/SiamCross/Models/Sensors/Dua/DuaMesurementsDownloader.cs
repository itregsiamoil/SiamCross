using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Du.Measurement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    class DuaMesurementsDownloader : IMeasurementsDownloader
    {
        private ISensor _Sensor;
        private IProtocolConnection _Connection;

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

        public DuaMesurementsDownloader(ISensor sensor)
        {
            _Sensor = sensor;
            _Connection = sensor.Connection;

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
        }
        public Task Clear()
        {
            MemStruct _CurrentAviable = new MemStruct(0x8418);

            return Task.CompletedTask;
            //CtrlReg.Value = 0x02;
            //await _Connection.ReadAsync(_CurrentAviable);
        }
        async Task<RespResult> SingleUpdate()
        {
            RespResult res = RespResult.ErrorUnknown;
            try
            {
                var aviable = new MemStruct(0x8418);
                aviable.Add(new MemVarUInt16(nameof(Uksh), 0, Uksh.Data as MemValueUInt16));
                aviable.Add(new MemVarUInt16(nameof(Ukex), 0, Ukex.Data as MemValueUInt16));

                var tmp = _Connection.AdditioonalTimeout;
                _Connection.AdditioonalTimeout = 2000;
                res = await _Connection.ReadAsync(aviable);
                _Connection.AdditioonalTimeout = tmp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION in write"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return res;
        }
        bool NeedRetry(RespResult result)
        {
            switch (result)
            {
                case RespResult.ErrorPkg:
                case RespResult.NormalPkg: return false;
                default:
                case RespResult.ErrorUnknown:
                case RespResult.ErrorConnection:
                case RespResult.ErrorSending:
                case RespResult.ErrorTimeout:
                case RespResult.ErrorCrc:
                    return true;
            }
        }
        public async Task<RespResult> Update()
        {
            if (!_Sensor.Activate)
                _Sensor.Activate = true;
            RespResult ret = RespResult.ErrorTimeout;
            for (int i = 0; i < 3; ++i)
            {
                ret = await SingleUpdate();
                if (!NeedRetry(ret))
                    break;
            }
            return ret;
        }
        public int AviableRep()
        {
            return Uksh.Value;
        }
        public int AviableEcho()
        {
            return Ukex.Value;
        }
        public async Task<IReadOnlyList<object>> Download(uint begin, uint qty
            , Action<float> onStepProgress = null, Action<string> onStepInfo = null)
        {
            onStepProgress?.Invoke(0.01f);
            onStepInfo?.Invoke("Скачивание измерений");
            // оценка количества байт для скачивания
            uint total_bytes = qty * (ReportHeader.Size + _EchoSize);
            uint readed_bytes = 0;

            var data_list = new List<DuMeasurementData>();


            for (UInt32 rec = 0; rec < qty; ++rec)
            {
                ReportHeader.Address = 0x82000000 + ReportHeader.Size * (begin + rec);
                await _Connection.ReadAsync(ReportHeader);

                readed_bytes += ReportHeader.Size;
                onStepProgress?.Invoke(readed_bytes / total_bytes);


                var well = Encoding.UTF8.GetString(skv.Value, 0, skv.Value.Length);
                var bush = Encoding.UTF8.GetString(kust.Value);

                var secp = new DuMeasurementSecondaryParameters(
                    _Sensor.Name
                    , Resource.Echogram
                    , field.Value.ToString()
                    , string.IsNullOrEmpty(well) ? "0" : well
                    , string.IsNullOrEmpty(bush) ? "0" : bush
                    , shop.Value.ToString()
                    , 0.0
                    , string.Empty
                    , _Sensor.Battery
                    , _Sensor.Temperature
                    , _Sensor.Firmware
                    , string.Empty
                    , "0", "0", "0"
                    );

                var startp = new DuMeasurementStartParameters(false, false, false, secp, 0.0);

                var echogramm = new MemVarByteArray(null, 0, new MemValueByteArray(_EchoSize));

                byte[] _currentEchogram = new byte[3000];

                DuMeasurementData data = new DuMeasurementData(DateTime.Now
                    , startp
                    , pressure.Value
                    , urov.Value
                    , kolotr.Value
                    , _currentEchogram
                    , MeasureState.Ok);

                data_list.Add(data);
            }



            return data_list;
        }


    }
}
