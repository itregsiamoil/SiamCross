using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementManager
    {
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

            sensor.Connection.DataReceived += _parser.ByteProcess;
            //_parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementRecieveHandler;
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
            byte[] resp = { };
            resp = await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.ResearchHeader]);
            if (16 > resp.Length)
                return false;
                
            _fluidLevel = BitConverter.ToInt16(resp, 12);
            _numberOfReflections = BitConverter.ToInt16(resp, 14);

            return true;
        }

        
        private async Task<DuMeasurementData> GetMeasurementData()
        {
            await DownloadEchogram();
            //await Task.Delay(300);
            await Sensor.Connection.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.StateZeroing]);
            //await Task.Delay(300);

            List<byte> echogramRawBytes = _currentEchogram.ToList();

            var data = new DuMeasurementData(echogramRawBytes,
                _fluidLevel, (float)Math.Round(_pressure, 1), 
                _numberOfReflections, DateTime.Now, _measurementParameters.SecondaryParameters);

            if(string.IsNullOrEmpty(data.SecondaryParameters.SoundSpeed))
            {
                var correctionTable = HandbookData.Instance.GetSoundSpeedList().Find(
                    x => x.ToString() == data.SecondaryParameters.SoundSpeedCorrection);
                data.SecondaryParameters.SoundSpeed = 
                    correctionTable.GetApproximatedSpeedFromTable(data.AnnularPressure).ToString();
            }

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
            float progress_size = (100f - _progress) / 2;

            UpdateProgress(_progress, "Read echogramm");
            Action<float> StepProgress = (float sep_cost) =>
            {
                _progress += sep_cost;
                UpdateProgress(_progress);
            };
            await ReadMemory(Sensor.Connection, _currentEchogram
                , 0, 0x81000000, (uint)_currentEchogram.Length , 50, StepProgress, progress_size);

        }
        
/*
        private async Task DownloadEchogram2()
        {
            byte[] length = BitConverter.GetBytes(20);
            var command = new List<byte>
            {
                0x0D, 0x0A,
                0x01, 0x01,
                0x00, 0x00, 0x00, 0x81,
            };
            command.Add(length[0]);
            command.Add(length[1]);

            AddCrc();

            //Read first 500 bytes

            await Sensor.Connection.SendData(command.ToArray());
            await Task.Delay(300);

            RemoveCrc();

            //Read rest
            for (int i = 0; i < 149; i++)
            {
                short newAdress = (short)(BitConverter.ToInt16(new byte[] { command[4], command[5] }, 0) + 20);
                byte[] newAdressBytes = BitConverter.GetBytes(newAdress);
                command[4] = newAdressBytes[0];
                command[5] = newAdressBytes[1];

                AddCrc();
                await Sensor.Connection.SendData(command.ToArray());
                await Task.Delay(150);

                RemoveCrc();
            }

            void AddCrc()
            {
                byte[] crc = CrcModbusCalculator.ModbusCrc(command.GetRange(2, 8).ToArray());
                command.Add(crc[0]);
                command.Add(crc[1]);
            }

            void RemoveCrc()
            {
                command.RemoveAt(command.Count - 1);
                command.RemoveAt(command.Count - 1);
            }
        }
        */

        private async Task Start()
        {
            System.Diagnostics.Debug.WriteLine("Execute start measure");
            await Sensor.Connection.Exchange(DuCommands.FullCommandDictionary[DuCommandsEnum.StartMeasurement]);
        }

        private async Task SendParameters()
        {
            string firstByte = _measurementParameters.Inlet ? "00000001" : "00000000";
            string secondByte = _measurementParameters.Amplification ? "00000001" : "00000000";

            byte[] data = new byte[] 
            { 
                Convert.ToByte(firstByte, 2), 
                Convert.ToByte(secondByte, 2) 
            };

            byte[] command = _commandGenerator.GenerateWriteCommand(
                DuCommands.FullCommandDictionary[DuCommandsEnum.Revbit], data);

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
        private async Task<DuMeasurementStatus> IsMeasurementDone()
        {
            await Start();


            DuMeasurementStatus status = DuMeasurementStatus.Empty;
            byte[] resp = { };
            UInt32 measure_time_sec = 1000;
            float sep_cost = 50f / measure_time_sec;
            bool isDone = false;
            for (UInt32 i = 0; i < measure_time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);
                status = await GetStatus();
                switch(status)
                {
                    default:
                    case DuMeasurementStatus.EсhoMeasurement:
                    case DuMeasurementStatus.WaitingForClick:
                    case DuMeasurementStatus.Empty: 
                        break;
                    case DuMeasurementStatus.NoiseMeasurement:
                        //await Start();
                        break;
                    case DuMeasurementStatus.Сompleted:
                        isDone = true;
                        break;
                }

                _progress += sep_cost;
                UpdateProgress(_progress, status.ToString());
            }
            return status;
        }

        private short _fluidLevel = 0;
        private short _numberOfReflections = 0;
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

        public void MeasurementRecieveHandler(DuCommandsEnum commandName, byte[] data)
        {
            switch (commandName)
            {
                case DuCommandsEnum.ResearchHeader:
                    if (data.Length == 4)
                    {
                        _fluidLevel = 
                            BitConverter.ToInt16(new byte[] { data[0], data[1]}, 0);
                        _numberOfReflections = 
                            BitConverter.ToInt16(new byte[] { data[2], data[3]}, 0);
                    }
                    break;
                case DuCommandsEnum.EchogramData:
                    //_currentEchogram.Add(data);
                    _progress += 0.666f;
                    break;
                case DuCommandsEnum.Pressure:
                    Debug.WriteLine("ANNULAR PRESSURE: " + BitConverter.ToString(data));
                    _pressure = BitConverter.ToInt16(new byte[] { data[0], data[1] }, 0) / 10.0f;
                    break;
                default:
                    break;
            }
        }
    }
}
