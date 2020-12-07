using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections;
using SiamCross.AppObjects;
using Autofac;
using SiamCross.Services.Logging;
using NLog;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementManager
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private DuMeasurementStartParameters _measurementParameters;
        private CommandGenerator _commandGenerator;
        private ISensor mSensor;

        public SensorData SensorData { get => mSensor.SensorData; }
        public ISensor Sensor { get => mSensor; }

        public DuMeasurementStatus MeasurementStatus { get; set; }

        byte[] _currentEchogram = new byte[3000]; 

        private DuParser _parser = new DuParser();

        public DuMeasurementManager(ISensor sensor,
            DuMeasurementStartParameters measurementParameters)
        {
            mSensor = sensor;
            _measurementParameters = measurementParameters;

            _commandGenerator = new CommandGenerator();

            //sensor.Connection.DataReceived += _parser.ByteProcess;
            //_parser.MessageReceived += ReceiveHandler;
            //_parser.ByteMessageReceived += MeasurementRecieveHandler;
        }

        public async Task<object> RunMeasurement()
        {
            Debug.WriteLine("SENDING PARAMETERS");
            await SendParameters();
            //await Task.Delay(300);
            await GetPressure();
            //await Task.Delay(300);
            MeasurementStatus = await IsMeasurementDone();
            //await Task.Delay(300);
            //await Sensor.Connection.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.SensorState]);
            //await Task.Delay(300);
            await DownloadHeader();
            //await Task.Delay(300);
            var result = await GetMeasurementData();
            //await Task.Delay(2000);
            return result;
        }

        private async Task<bool> GetPressure()
        {
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure]);
            if (14 > resp.Length)
                return false;
            _pressure = (float)BitConverter.ToInt16(resp, 12)/10.0f;
            Debug.WriteLine("ANNULAR PRESSURE: " + _pressure.ToString());
            return true;
        }




        private async Task<bool> DownloadHeader()
        {
            UpdateProgress(_progress, "Read header");
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.ResearchHeader]);
            if (16 > resp.Length)
                return false;
                
            _fluidLevel = BitConverter.ToUInt16(resp, 12);
            if(_measurementParameters.Depth6000)
            {
                BitVector32 myBV = new BitVector32(_fluidLevel);
                int bit0 = BitVector32.CreateMask(00000000);
                myBV[0x4000] = false;
                _fluidLevel = (ushort)(myBV.Data);
            }


            _numberOfReflections = BitConverter.ToUInt16(resp, 14);

            return true;
        }

        
        private async Task<DuMeasurementData> GetMeasurementData()
        {
            await DownloadEchogram();
            //await Task.Delay(300);
            _logger.Trace("set StateZeroing");
            await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.StateZeroing]);
            //await Task.Delay(300);

            _logger.Trace("set raw to list");
            List<byte> echogramRawBytes = _currentEchogram.ToList();

            _logger.Trace("begin create report");
            var data = new DuMeasurementData(echogramRawBytes,
                _fluidLevel, (float)Math.Round(_pressure, 1), 
                _numberOfReflections, DateTime.Now, _measurementParameters.SecondaryParameters);

            _logger.Trace("end create report");
            if (string.IsNullOrEmpty(data.SecondaryParameters.SoundSpeed))
            {
                var correctionTable = HandbookData.Instance.GetSoundSpeedList().Find(
                    x => x.ToString() == data.SecondaryParameters.SoundSpeedCorrection);
                data.SecondaryParameters.SoundSpeed = 
                    correctionTable.GetApproximatedSpeedFromTable(data.AnnularPressure).ToString();
            }
            _logger.Trace("recalc fluid level");
            data.FluidLevel = (int)(data.FluidLevel *
                float.Parse(data.SecondaryParameters.SoundSpeed) / 341.333f);

            return data;
        }
        

        static private async Task<bool> ReadMemory(IProtocolConnection conn, byte[] dst, int start
            , UInt32 start_addr, UInt32 mem_size, UInt16 step_size
            , Action<float> DoStepProgress, float progress_size)
        {
            if (null == dst || dst.Length < (int)mem_size)
                throw new Exception("dst is too short");

            //const UInt32 start_addr = 0x81000000;
            //const UInt32 mem_size = 1000*2;
            //const UInt32 step_size = 20;
            UInt32 step_count = mem_size / step_size;

            float sep_cost = (progress_size) / step_count;

            byte[] cmd = new byte[]
            {
                0x0D, 0x0A, //header
                0x01, 0x01, // device // command
                0x00, 0x00, 0x00, 0x81, // address
                0x00, 0x00, // length
                0xFF, 0xFF, // crc

            };
            byte[] resp = { };
            UInt32 curr_addr = 0;

            while (mem_size > curr_addr)
            {
                UInt32 addr = curr_addr + start_addr;
                BitConverter.GetBytes(addr).CopyTo(cmd.AsSpan(4, 4)); //addr
                BitConverter.GetBytes(step_size).CopyTo(cmd.AsSpan(8, 2)); //len
                byte[] crc = CrcModbusCalculator.ModbusCrc(cmd, 2, cmd.Length - 4);
                crc.CopyTo(cmd.AsSpan(10, 2)); //crc

                resp = await conn.Exchange(cmd);
                if (0 == resp.Length)
                    return false;

                resp.AsSpan(12, (int)step_size).CopyTo(dst.AsSpan((int)curr_addr, (int)step_size));
                DoStepProgress(sep_cost);

                UInt32 next_curr_addr = curr_addr + step_size;
                if (mem_size > next_curr_addr)
                    curr_addr = next_curr_addr;
                else
                    curr_addr += mem_size - curr_addr;
            }

            return true;
        }
        private async Task DownloadEchogram()
        {
            _logger.Trace("begin read echogramm");
            float progress_size = (100f - _progress) ;

            UpdateProgress(_progress, "Read echogramm");
            Action<float> StepProgress = (float sep_cost) =>
            {
                _progress += sep_cost;
                UpdateProgress(_progress);
            };
            await ReadMemory(Sensor.Connection, _currentEchogram
                , 0, 0x81000000, (uint)_currentEchogram.Length , 50, StepProgress, progress_size);

            _logger.Trace("end read echogramm");
        }

        private async Task SendParameters()
        {
            BitVector32 myBV = new BitVector32(0);
            int bit0 = BitVector32.CreateMask();
            int bit1 = BitVector32.CreateMask(bit0);
            int bit2 = BitVector32.CreateMask(bit1);
            int bit3 = BitVector32.CreateMask(bit2);
            int bit4 = BitVector32.CreateMask(bit3);
            int bit5 = BitVector32.CreateMask(bit4);
            int bit6 = BitVector32.CreateMask(bit5);
            int bit7 = BitVector32.CreateMask(bit6);
            int bit8 = BitVector32.CreateMask(bit7);
            int bit9 = BitVector32.CreateMask(bit8);

            myBV[bit0] = _measurementParameters.Inlet;
            myBV[bit6] = _measurementParameters.Depth6000;
            myBV[bit9] = _measurementParameters.Amplification;
            byte[] my_data = BitConverter.GetBytes(myBV.Data).AsSpan(0,2).ToArray();

            byte[] command = _commandGenerator.GenerateWriteCommand(
                DuCommands.FullCommandDictionary[DuCommandsEnum.Revbit], my_data);

           // _writeLog(command);
           await Sensor.Connection.Exchange(command);
            
        }

        private async Task<DuMeasurementStatus> GetStatus()
        {
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.SensorState]);
            if (0 != resp.Length)
                status = (DuMeasurementStatus)BitConverter.ToUInt16(resp, 12);
            System.Diagnostics.Debug.WriteLine("DU status="+ status.ToString());
            return status;
        }

        private async Task<bool> Start()
        {
            System.Diagnostics.Debug.WriteLine("Execute start measure");
            await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.StartMeasurement]);
            UInt32 time_sec = 100;
            float sep_cost = 10f / time_sec;
            bool isDone = false;
            for (UInt32 i = 0; i < time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);
                DuMeasurementStatus status = await GetStatus();
                switch (status)
                {
                    case DuMeasurementStatus.WaitingForClick:
                        isDone = true;
                        break;
                    default:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.Empty:
                    case DuMeasurementStatus.NoiseMeasurement:
                    case DuMeasurementStatus.Сompleted:
                        break;
                }
                _progress += sep_cost;
                UpdateProgress(_progress, status.ToString());
            }
            return isDone;

        }
        private async Task<DuMeasurementStatus> IsMeasurementDone()
        {
            bool started = await Start();
            
            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            byte[] resp = { };
            UInt32 measure_time_sec = 40;//18/36
            float sep_cost = 50f / measure_time_sec;
            bool isDone = false;
            for (UInt32 i = 0; i < measure_time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);
                status = await GetStatus();
                switch(status)
                {
                    case DuMeasurementStatus.Сompleted:
                        isDone = true;
                        break;
                    default:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.WaitingForClick:
                    case DuMeasurementStatus.Empty: 
                    case DuMeasurementStatus.NoiseMeasurement:
                        break;
                }

                _progress += sep_cost;
                UpdateProgress(_progress, status.ToString());
            }
            return status;
        }

        private UInt16 _fluidLevel = 0;
        private UInt16 _numberOfReflections = 0;
        private float _pressure;

        private void UpdateProgress(float pos)
        {
            _progress = pos;
            Sensor.MeasureProgress = _progress / 100;
        }

        private void UpdateProgress(float pos, string text)
        {
            SensorData.Status = "measure: " + text;
            UpdateProgress(pos);
        }
        private float _progress = 0;
        public int Progress
        {
            get
            {
                return (int)_progress;
            }
        }
  
    }
}
