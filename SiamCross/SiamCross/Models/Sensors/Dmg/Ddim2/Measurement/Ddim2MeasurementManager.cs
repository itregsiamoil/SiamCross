﻿using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg.Ddim2.Measurement
{
    public class Ddim2MeasurementManager
    {
        private IProtocolConnection _bluetoothAdapter;
        private CommandGenerator _configGenerator;
        private Ddim2MeasurementStartParameters _measurementParameters;
        private DmgBaseMeasureReport _report;
        private DynStructuredContainer _dynContainer;
        public SensorData SensorData { get; private set; }
        public byte[] ErrorCode { get; private set; }
        public DmgMeasureStatus MeasurementStatus { get; set; }

        private List<byte[]> _currentDynGraph;
        private List<byte[]> _currentAccelerationGraph;

        public Ddim2MeasurementManager(IProtocolConnection bluetoothAdapter, SensorData sensorData, 
                Ddim2MeasurementStartParameters measurementParameters)
        {
            _bluetoothAdapter = bluetoothAdapter;
            _measurementParameters = measurementParameters;
            _configGenerator = new CommandGenerator();

            SensorData = sensorData;

            _currentDynGraph = new List<byte[]>();
            _currentAccelerationGraph = new List<byte[]>();
        }

        public async Task<object> RunMeasurement()
        {
            await Task.Delay(300);
            await SendParameters();
            await Start();
            await IsMeasurementDone();

            bool gotError = false;

            if (MeasurementStatus == DmgMeasureStatus.Error)
            {
                gotError = true;
                await ReadErrorCode();
            }

            var fullReport = await DownloadMeasurement(gotError);
            return fullReport;
        }

        private async Task SendParameters()
        {
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

                await _bluetoothAdapter.SendData(DmgCmd.Get("ReadDeviceStatus"));

                if (MeasurementStatus == DmgMeasureStatus.Ready
                   || MeasurementStatus == DmgMeasureStatus.Error)
                {
                    isDone = true;
                }
            }

            return isDone;
        }

        private async Task Start()
        {
            await _bluetoothAdapter.SendData(DmgCmd.Get("InitializeMeasurement"));
            await Task.Delay(300);
            await _bluetoothAdapter.SendData(DmgCmd.Get("StartMeasurement"));
            await Task.Delay(300);
        }

        public async Task ReadErrorCode()
        {
            await _bluetoothAdapter.SendData(DmgCmd.Get("ReadMeasurementErrorCode"));
            //await Task.Delay(300);
        }

        public async Task ReadMeasurementHeader()
        {
            do
            {
                await _bluetoothAdapter.SendData(DmgCmd.Get("ReadMeasurementReport"));
                await Task.Delay(400);
            }
            while (_report == null);
        }

        public async Task<Ddim2MeasurementData> DownloadMeasurement(bool isError)
        {
            var _currentReport = new List<byte>();
            
            _currentDynGraph.Add(new byte[2] { 0, 0 });
            var _currentAccelerationGraph = new List<byte[]>();

            await ReadMeasurementHeader();

            await GetDgm4kB();

            await Task.Delay(500);

            var dynRawBytes = new List<byte>();
            foreach (var bytes in _dynContainer.GetDynData())
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

            await _bluetoothAdapter.SendData(DmgCmd.Get("InitializeMeasurement"));
            //await Task.Delay(Constants.ShortDelay);
            return measurement;
        }

        private async Task GetDgm4kB()
        {

            _dynContainer = new DynStructuredContainer();
            var addresses = _dynContainer.GetEmptyAddresses();
            while (addresses.Count != 0)
            {
                System.Diagnostics.Debug.WriteLine($"Empty addresses = {addresses.Count}");

                byte[] length = BitConverter.GetBytes(20);
                var command = new List<byte>
                    {
                        0x0D, 0x0A,
                        0x01, 0x01,
                    };
                command.AddRange(addresses[0]);
                command.Add(length[0]);
                command.Add(length[1]);

                AddCrc();

                await _bluetoothAdapter.SendData(command.ToArray());
                await Task.Delay(Constants.LongDelay);

                RemoveCrc();

                for (int i = 1; i < addresses.Count; i++)
                {
                    command[4] = addresses[i][0];
                    command[5] = addresses[i][1];

                    AddCrc();
                    await _bluetoothAdapter.SendData(command.ToArray());
                    //await Task.Delay(Constants.LongDelay);

                    RemoveCrc();
                }
                await Task.Delay(1000);
                addresses = _dynContainer.GetEmptyAddresses();
                #region AddRemoveCRC
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
                #endregion
            }

        }

        private float _progress = 0;
        public int Progress
        {
            get
            {
                return (int)_progress;
            }
        }

        public void MemoryRecieveHandler(byte[] address, byte[] data)
        {
            if(_dynContainer.AddData(address, data))
            {
                _progress += 0.5f;
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
                    var report = new List<UInt16>();
                    for (int i = 0; i + 1 < data.Count(); i += 2)
                    {
                        var array = new byte[] { data[i], data[i + 1] };
                        UInt16 value = BitConverter.ToUInt16(array, 0);
                        report.Add(value);
                    }
                    _report = new DmgBaseMeasureReport(
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
                default:
                    break;
            }
        }
    }
}