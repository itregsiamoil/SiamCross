﻿using SiamCross.Models.Sensors.Dynamographs;
using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement
{
    public class SiddosA3MMeasurementManager
    {
        private IBluetoothAdapter _bluetoothAdapter;
        private SiddosA3MConfigCommandsGenerator _configGenerator;
        private SiddosA3MMeasurementStartParameters _measurementParameters;
        private SiddosA3MMeasurementReport _report;
        private DynStructuredContainer _dynContainer;
        public SensorData SensorData { get; private set; }
        public byte[] ErrorCode { get; private set; }
        public DynamographMeasurementStatus MeasurementStatus { get; set; }

        private List<byte[]> _currentDynGraph;
        private List<byte[]> _currentAccelerationGraph;

        public SiddosA3MMeasurementManager(IBluetoothAdapter bluetoothAdapter, SensorData sensorData,
                SiddosA3MMeasurementStartParameters measurementParameters)
        {
            _bluetoothAdapter = bluetoothAdapter;
            _measurementParameters = measurementParameters;
            _configGenerator = new SiddosA3MConfigCommandsGenerator();

            SensorData = sensorData;

            _currentDynGraph = new List<byte[]>();
            _currentAccelerationGraph = new List<byte[]>();
        }

        public async Task<object> RunMeasurement()
        {
            //Task.Delay(Constants.LongDelay);
            if (!await SendParameters())
                return null;
            if(!await Start())
                return null;
            
            if (!await IsMeasurementDone())
                return null;

            bool gotError = false;

            if (MeasurementStatus == DynamographMeasurementStatus.Error)
            {
                gotError = true;
                await ReadErrorCode();
            }

            var fullReport = await DownloadMeasurement(gotError);
            return fullReport;
        }

        protected void UpdateProgress(int pos, string text)
        {
            _progress = pos;
            SensorData.Status = "measure ["+ pos.ToString() + "%] - "+ text;
        }


        private async Task<bool> SendParameters()
        {
            UpdateProgress(1, "Send Parameters");
            byte[] resp = { };
            //await Task.Delay(Constants.LongDelay);
            resp = await _bluetoothAdapter.Exchange(_configGenerator.SetDynPeriod(_measurementParameters.DynPeriod));
            if (0 == resp.Length)
                return false;
            //await Task.Delay(Constants.LongDelay);
            resp = await _bluetoothAdapter.Exchange(_configGenerator.SetApertNumber(_measurementParameters.ApertNumber));
            if (0 == resp.Length)
                return false;
            //await Task.Delay(Constants.LongDelay);
            resp = await _bluetoothAdapter.Exchange(_configGenerator.SetImtravel(_measurementParameters.Imtravel));
            if (0 == resp.Length)
                return false;
            //await Task.Delay(Constants.LongDelay);
            resp = await _bluetoothAdapter.Exchange(_configGenerator.SetModelPump(_measurementParameters.ModelPump));
            if (0 == resp.Length)
                return false;
            //await Task.Delay(Constants.LongDelay);
            return true;
        }

        private async Task<bool> Start()
        {
            UpdateProgress(2, "Send Init and Start");
            byte[] resp = { };
            resp = await _bluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["InitializeMeasurement"]);
            if (0 == resp.Length)
                return false;
            //await Task.Delay(Constants.LongDelay);
            resp = await _bluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["StartMeasurement"]);
            if (0 == resp.Length)
                return false;
            //await Task.Delay(Constants.LongDelay);
            return true;
        }

        private async Task<bool> IsMeasurementDone()
        {
            byte[] resp = { };
            string dataValue;
            Ddim2.Ddim2Parser pp = new Ddim2.Ddim2Parser();

            bool isDone = false;
            for (int i = 0; i < 50 && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);
                resp = await _bluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]); ;
                if (0 == resp.Length)
                    return false;

                dataValue = pp.ConvertToStringPayload(resp);
                MeasurementStatus = DynamographStatusAdapter.StringStatusToEnum(dataValue);
                UpdateProgress(3 + i, "measure "+ MeasurementStatus);


                if (MeasurementStatus == DynamographMeasurementStatus.Ready
                   || MeasurementStatus == DynamographMeasurementStatus.Error)
                {
                    isDone = true;
                }
            }

            return isDone;
        }



        public async Task<bool> ReadErrorCode()
        {
            byte[] resp = { };
            resp = await _bluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["ReadMeasurementErrorCode"]);
            if (0 == resp.Length)
                return false;
            ErrorCode = resp.AsSpan().Slice(12, 4).ToArray();
            return true;
            //await Task.Delay(Constants.LongDelay);
        }

        public async Task ReadMeasurementHeader()
        {
            do
            {
                await _bluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadMeasurementReport"]);
                await Task.Delay(400);
            }
            while (_report == null);
        }

        public async Task<SiddosA3MMeasurementData> DownloadMeasurement(bool isError)
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

            SiddosA3MMeasurementData measurement = isError ?
                new SiddosA3MMeasurementData(_report,
                    (short)_measurementParameters.ApertNumber,
                    (short)_measurementParameters.ModelPump,
                    dynRawBytes,
                    DateTime.Now,
                    _measurementParameters.SecondaryParameters,
                    null,
                    ErrorCode) :
                new SiddosA3MMeasurementData(
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
            await Task.Delay(Constants.LongDelay);
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
            if (_dynContainer.AddData(address, data))
            {
                _progress += 0.5f;
            }
        }

        public void MeasurementRecieveHandler(string commandName, byte[] data)
        {
            if (data == null)
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
                    _report = new SiddosA3MMeasurementReport(
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
