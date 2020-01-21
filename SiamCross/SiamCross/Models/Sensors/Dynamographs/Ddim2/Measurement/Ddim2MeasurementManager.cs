﻿using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Tools;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dynamographs.Ddim2.Measurement
{
    public class Ddim2MeasurementManager
    {
        private IBluetoothAdapter _bluetoothAdapter;
        private Ddim2DeviceConfigCommandGenerator _configGenerator;
        private Ddim2MeasurementStartParameters _measurementParameters;
        private Ddim2MeasurementReport _report;
        public SensorData SensorData { get; private set; }
        public byte[] ErrorCode { get; private set; }
        public DynamographMeasurementStatus MeasurementStatus { get; set; }

        private List<byte[]> _currentDynGraph;
        private List<byte[]> _currentAccelerationGraph;

        public Ddim2MeasurementManager(IBluetoothAdapter bluetoothAdapter, SensorData sensorData, 
                Ddim2MeasurementStartParameters measurementParameters)
        {
            _bluetoothAdapter = bluetoothAdapter;
            _measurementParameters = measurementParameters;
            _configGenerator = new Ddim2DeviceConfigCommandGenerator();
            SensorData = sensorData;

            _currentDynGraph = new List<byte[]>();
            _currentAccelerationGraph = new List<byte[]>();
        }

        public async Task<object> RunMeasurement()
        {
            await SendParameters();
            await Start();
            await IsMeasurementDone();

            bool gotError = false;

            if (MeasurementStatus == DynamographMeasurementStatus.Error)
            {
                gotError = true;
                await ReadErrorCode();
            }

            var fullReport = await DownloadMeasurement(gotError);
            return fullReport;
        }

        private async Task SendParameters()
        {
            await _bluetoothAdapter.SendData(_configGenerator.SetRod(_measurementParameters.Rod));
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(_configGenerator.SetDynPeriod(_measurementParameters.DynPeriod));
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(_configGenerator.SetApertNumber(_measurementParameters.ApertNumber));
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(_configGenerator.SetImtravel(_measurementParameters.Imtravel));
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(_configGenerator.SetModelPump(_measurementParameters.ModelPump));
            await Task.Delay(300);
        }

        private async Task<bool> IsMeasurementDone()
        {
            bool isDone = false;
            while (!isDone)
            {
                await Task.Delay(300);

                await _bluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]);

                if (MeasurementStatus == DynamographMeasurementStatus.Ready
                   || MeasurementStatus == DynamographMeasurementStatus.Error)
                {
                    isDone = true;
                }
            }

            return isDone;
        }

        private async Task Start()
        {
            await _bluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["InitializeMeasurement"]);
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["StartMeasurement"]);
            await Task.Delay(300);
        }

        public async Task ReadErrorCode()
        {
            await _bluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadMeasurementErrorCode"]);
            await Task.Delay(300);
        }

        /*/ Copy from SiamBLE /*/
        public async Task<Ddim2MeasurementData> DownloadMeasurement(bool isError)
        {
            var _currentReport = new List<byte>();
            
            _currentDynGraph.Add(new byte[2] { 0, 0 });
            var _currentAccelerationGraph = new List<byte[]>();

            await _bluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadMeasurementReport"]);

            await GetDgm4kB();

            var dynRawBytes = new List<byte>();
            foreach (var bytes in _currentDynGraph)
            {
                foreach (var b in bytes)
                {
                    dynRawBytes.Add(b);
                }
            }

            Ddim2MeasurementData measurement = isError ? 
                new Ddim2MeasurementData(_report,
                    (short)_measurementParameters.ApertNumber,
                    (short)_measurementParameters.ModelPump,
                    dynRawBytes,
                    DateTime.Now,
                    _measurementParameters.SecondaryParameters,
                    null,
                    ErrorCode) : 
                new Ddim2MeasurementData(
                    _report,
                    (short)_measurementParameters.ApertNumber,
                    (short)_measurementParameters.ModelPump,
                    dynRawBytes,
                    DateTime.Now,
                    _measurementParameters.SecondaryParameters,
                    null,
                    null);                   

            var dynGraphPoints = DgmConverter.GetXYs(measurement.DynGraph.ToList(),
                    measurement.Report.Step, measurement.Report.WeightDiscr);

            measurement.DynGraphPoints = dynGraphPoints;

            await _bluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["InitializeMeasurement"]);
            await Task.Delay(300);
            return measurement;
        }

        private async Task GetDgm4kB()
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
            await Task.Delay(200);

            RemoveCrc();

            //Read rest
            for (int i = 0; i < 198; i++)
            {
                short newAdress = (short)(BitConverter.ToInt16(new byte[] { command[4], command[5] }, 0) + 20);
                byte[] newAdressBytes = BitConverter.GetBytes(newAdress);
                command[4] = newAdressBytes[0];
                command[5] = newAdressBytes[1];

                AddCrc();
                await _bluetoothAdapter.SendData(command.ToArray());
                await Task.Delay(200);

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

        public void MeasurementRecieveHandler(string commandName, byte[] data)
        {
            if(data == null)
            {
                return;
            }

            switch (commandName)
            {
                case "ExportDynGraph":
                    Console.WriteLine(BitConverter.ToString(data));
                    _currentDynGraph.Add(data);
                    break;
                case "ExportAccelerationGraph":
                    _currentAccelerationGraph.Add(data.Reverse().ToArray());
                    break;
                case "ReadMeasurementReport":
                    var report = new List<short>();
                    for (int i = 0; i + 1 < data.Count(); i += 2)
                    {
                        var array = new byte[] { data[i], data[i + 1] };
                        short value = BitConverter.ToInt16(array, 0);
                        report.Add(value);
                    }
                    _report = new Ddim2MeasurementReport(
                        report[0],
                        report[1],
                        report[2],
                        report[3],
                        report[4],
                        report[5],
                        report[6]);
                    break;
                case "ReadMeasurementErrorCode":
                    ErrorCode = data;
                    break;
                case "DgmPart1":
                    Console.WriteLine($"Added {data.Length} bytes");
                    _currentDynGraph.Add(data);
                    
                    break;
                default:
                    break;
            }
        }
    }
}