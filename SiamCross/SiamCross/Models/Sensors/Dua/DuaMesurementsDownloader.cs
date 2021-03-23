using SiamCross.Models.Connection.Protocol;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    class DuaMesurementsDownloader : IMeasurementsDownloader
    {
        private ISensor _Sensor;
        private IProtocolConnection _Connection;

        public readonly MemStruct _CurrentParam;//0x8400
        public readonly MemVarUInt16 BatteryVoltage;
        public readonly MemVarUInt16 ТempC;
        public readonly MemVarInt16 Pressure;
        public readonly MemVarByteArray Time;
        public readonly MemVarUInt16 Emak;
        public readonly MemVarUInt16 Rdav;
        public readonly MemVarByteArray TimeRequre;
        public readonly MemVarUInt8 Interv;
        public readonly MemVarUInt16 Kolt;
        public readonly MemVarUInt16 Timeawt;
        public readonly MemVarUInt16 Uksh;
        public readonly MemVarUInt16 Ukex;


        public DuaMesurementsDownloader(ISensor sensor)
        {
            _Sensor = sensor;
            _Connection = sensor.Connection;

            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            ТempC = _CurrentParam.Add(new MemVarUInt16(nameof(ТempC)));
            Pressure = _CurrentParam.Add(new MemVarInt16(nameof(Pressure)));
            Time = _CurrentParam.Add(new MemVarByteArray(nameof(Time),0,new MemValueByteArray(8)));
            Emak = _CurrentParam.Add(new MemVarUInt16(nameof(Emak)));
            Rdav = _CurrentParam.Add(new MemVarUInt16(nameof(Rdav)));
            TimeRequre = _CurrentParam.Add(new MemVarByteArray(nameof(TimeRequre), 0, new MemValueByteArray(3)));
            Interv = _CurrentParam.Add(new MemVarUInt8(nameof(Interv)));
            Kolt = _CurrentParam.Add(new MemVarUInt16(nameof(Kolt)));
            Timeawt = _CurrentParam.Add(new MemVarUInt16(nameof(Timeawt)));
            Uksh = _CurrentParam.Add(new MemVarUInt16(nameof(Uksh)));
            Ukex = _CurrentParam.Add(new MemVarUInt16(nameof(Ukex)));
        }

        public async Task Clear()
        {
            MemStruct _CurrentAviable= new MemStruct(0x8418);


            //CtrlReg.Value = 0x02;
            //await _Connection.ReadAsync(_CurrentAviable);
        }
        public async Task Update()
        {
            var aviable = new MemStruct(0x8418);
            aviable.Add(new MemVarUInt16(nameof(Uksh), 0, Uksh.Data as MemValueUInt16));
            aviable.Add(new MemVarUInt16(nameof(Ukex), 0, Ukex.Data as MemValueUInt16));
            
            var tmp = _Connection.AdditioonalTimeout;
            _Connection.AdditioonalTimeout = 2000;
            await _Connection.ReadAsync(aviable);
            _Connection.AdditioonalTimeout = tmp;
        }
        public int Aviable()
        {
            return Uksh.Value+ Ukex.Value;
        }
        public int AviableEcho()
        {
            return Ukex.Value;
        }
        public async Task<object> Download(int begin, int end
            , Action<float> onStepProgress = null, Action<string> onStepInfo = null)
        {
            onStepProgress?.Invoke(0.01f);
            onStepInfo?.Invoke("Download SurvayParam");
          
            return null;
        }
    }
}
