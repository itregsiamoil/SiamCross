using SiamCross.Models.Sensors.Dynamographs.Ddim2;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du.Measurement
{
    public class DuMeasurementManager
    {
        private IBluetoothAdapter _bluetoothAdapter;
        private DuMeasurementStartParameters _measurementParameters;
        private CommandGenerator _commandGenerator;

        public SensorData SensorData { get; private set; }

        public DuMeasurementStatus MeasurementStatus { get; set; }

        private List<byte[]> _currentEchogram;

        public DuMeasurementManager(IBluetoothAdapter bluetoothAdapter, SensorData sensorData,
            DuMeasurementStartParameters measurementParameters)
        {
            _bluetoothAdapter = bluetoothAdapter;
            SensorData = sensorData;
            _measurementParameters = measurementParameters;

            _commandGenerator = new CommandGenerator();
            _currentEchogram = new List<byte[]>();
        }

        public async Task<object> RunMeasurement()
        {
            Debug.WriteLine("SENDING PARAMETERS");
            await SendParameters();
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.Pressure]);
            await Task.Delay(300);
            await Start();
            await Task.Delay(300);
            await IsMeasurementDone();
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.SensorState]);
            await Task.Delay(300);
            await DownloadHeader();
            await Task.Delay(300);
            var result = await GetMeasurementData();
            await Task.Delay(2000);
            return result;
        }

        private async Task DownloadHeader()
        {
            await _bluetoothAdapter.SendData(
                DuCommands.FullCommandDictionary[DuCommandsEnum.ResearchHeader]);
        }

        private async Task<DuMeasurementData> GetMeasurementData()
        {
            await DownloadEchogram();
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.StateZeroing]);
            await Task.Delay(300);

            var echogramRawBytes = new List<byte>();
            foreach (var bytes in _currentEchogram)
            {
                foreach (var b in bytes)
                {
                    echogramRawBytes.Add(b);
                }
            }

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

        private async Task DownloadEchogram()
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

            await _bluetoothAdapter.SendData(command.ToArray());
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
                await _bluetoothAdapter.SendData(command.ToArray());
                await Task.Delay(150);

                RemoveCrc();
            }

            void AddCrc()
            {
                var crcCalculator = new CrcModbusCalculator();
                byte[] crc = crcCalculator.ModbusCrc(command.GetRange(2, 8).ToArray());
                command.Add(crc[0]);
                command.Add(crc[1]);
            }

            void RemoveCrc()
            {
                command.RemoveAt(command.Count - 1);
                command.RemoveAt(command.Count - 1);
            }
        }

        private async Task Start()
        {
            //_writeLog(DuCommands.FullCommandDictionary[DuCommandsEnum.StartMeasurement]);
            await _bluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.StartMeasurement]);
            await Task.Delay(300);
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

            await _bluetoothAdapter.SendData(command);
            await Task.Delay(Constants.ShortDelay);
        }

        private async Task<bool> IsMeasurementDone()
        {
            bool isDone = false;
            short restartCounter = 0;
            while (!isDone)
            {
                await Task.Delay(300);
                await _bluetoothAdapter.SendData(DuCommands.FullCommandDictionary[DuCommandsEnum.SensorState]);

                if (MeasurementStatus == DuMeasurementStatus.Сompleted)
                {
                    isDone = true;
                }
                if(MeasurementStatus == DuMeasurementStatus.Empty)
                {
                    restartCounter++;
                    if(restartCounter == 5)
                    {
                        restartCounter = 0;
                        await Start();
                    }
                }
            }

            return isDone;
        }

        private short _fluidLevel = 0;
        private short _numberOfReflections = 0;
        private float _pressure;

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
                //case DuCommandsEnum.SensorState:
                //    short status = BitConverter.ToInt16(data, 0);
                //    MeasurementStatus = (DuMeasurementStatus)status;
                //    break;
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
                    _currentEchogram.Add(data);
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
