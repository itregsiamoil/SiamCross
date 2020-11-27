using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dmg.SiddosA3M.SiddosA3MMeasurement
{
    public class SiddosA3MMeasurementManager
    {
        private IProtocolConnection _bluetoothAdapter;
        private SiddosA3MConfigCommandsGenerator _configGenerator;
        private SiddosA3MMeasurementStartParameters _measurementParameters;
        private SiddosA3MMeasurementReport _report;
        private ISensor mSensor;
        private DynStructuredContainer _dynContainer;
        public SensorData SensorData { get => mSensor.SensorData; }
        public ISensor Sensor { get => mSensor; }
        public byte[] ErrorCode { get; private set; }
        public DmgMeasureStatus MeasurementStatus { get; set; }

        private List<byte[]> _currentDynGraph;
        private List<byte[]> _currentAccelerationGraph;

        public SiddosA3MMeasurementManager(IProtocolConnection bluetoothAdapter, ISensor sensor,
                SiddosA3MMeasurementStartParameters measurementParameters)
        {
            _bluetoothAdapter = bluetoothAdapter;
            _measurementParameters = measurementParameters;
            _configGenerator = new SiddosA3MConfigCommandsGenerator();
            mSensor = sensor;

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

            if (MeasurementStatus == DmgMeasureStatus.Error)
            {
                gotError = true;
                await ReadErrorCode();
            }

            var fullReport = await DownloadMeasurement(gotError);
            return fullReport;
        }

        private void UpdateProgress(float pos, string text)
        {
            _progress = pos;
            SensorData.Status = "measure: "+ text;
            Sensor.MeasureProgress = _progress / 100;
        }
        private float _progress = 0;
        public int Progress
        {
            get
            {
                return (int)_progress;
            }
        }

        private async Task<bool> SendParameters()
        {
            UpdateProgress(1, "Send Parameters");
            Console.WriteLine("SENDING PARAMETERS");
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
            Console.WriteLine("PARAMETERS HAS BEEN SENT");
            return true;
        }
        private async Task<bool> Start()
        {
            UpdateProgress(2, "Send Init and Start");
            byte[] resp = { };
            //resp = await _bluetoothAdapter.Exchange(DynamographCommands.FullCommandDictionary["InitializeMeasurement"]);
            //if (0 == resp.Length)
            //    return false;
            //await Task.Delay(Constants.LongDelay);
            resp = await _bluetoothAdapter.Exchange(DmgCmd.Get("StartMeasurement"));
            if (0 == resp.Length)
                return false;
            //await Task.Delay(Constants.LongDelay);
            Console.WriteLine("MEASUREMENT STARTED");
            return true;
        }
        private async Task<bool> IsMeasurementDone()
        {
            byte[] resp = { };
            string dataValue;
            Ddim2.Ddim2Parser pp = new Ddim2.Ddim2Parser();

            int measure_time_sec = _measurementParameters.DynPeriod * 2 / 1000;
            float sep_cost = 50 / measure_time_sec;

            bool isDone = false;
            for (int i = 0; i < measure_time_sec && !isDone; i++)
            {
                await Task.Delay(Constants.SecondDelay);
                resp = await _bluetoothAdapter.Exchange(DmgCmd.Get("ReadDeviceStatus")); ;
                if (0 == resp.Length)
                    return false;

                dataValue = Ddim2.Ddim2Parser.ConvertToStringPayload(resp);
                MeasurementStatus = DmgMeasureStatusAdapter.StringStatusToEnum(dataValue);
                _progress += sep_cost;
                UpdateProgress(_progress, MeasurementStatus.ToString());

                if (MeasurementStatus == DmgMeasureStatus.Ready
                   || MeasurementStatus == DmgMeasureStatus.Error)
                {
                    isDone = true;
                }
            }

            return isDone;
        }
        public async Task<bool> ReadErrorCode()
        {
            byte[] resp = { };
            resp = await _bluetoothAdapter.Exchange(DmgCmd.Get("ReadMeasurementErrorCode"));
            if (0 == resp.Length)
                return false;
            ErrorCode = resp.AsSpan().Slice(12, 4).ToArray();
            return true;
            //await Task.Delay(Constants.LongDelay);
        }

        public async Task<bool> ReadMeasurementHeader()
        {
            UpdateProgress(_progress, "Read Measurement Header");
            int retry = 3;
            bool is_ok = false;
            for(int r=0; r < retry && !is_ok; r++)
            {
                byte[] message = await _bluetoothAdapter.Exchange(DmgCmd.Get("ReadMeasurementReport"));
                if (null == message || 0 == message.Length)
                    continue;

                var data = Ddim2.Ddim2Parser.GetPayload(message);
                var report = new List<UInt16>();
                for (int i = 0; i + 1 < data.Count(); i += 2)
                {
                    var array = new byte[] { data[i], data[i + 1] };
                    UInt16 value = BitConverter.ToUInt16(array, 0);
                    report.Add(value);
                }
                _report = new SiddosA3MMeasurementReport(
                    report[0], report[1], report[2], report[3],
                    report[4], report[5], report[6]);
                is_ok = true;
            }//for
            return is_ok;
        }

        public async Task<SiddosA3MMeasurementData> DownloadMeasurement(bool isError)
        {
            UpdateProgress(_progress, "Download measurement");

            var _currentReport = new List<byte>();

            _currentDynGraph.Add(new byte[2] { 0, 0 });
            var _currentAccelerationGraph = new List<byte[]>();

            await ReadMeasurementHeader();

            await GetDgm4kB_2();

            //await Task.Delay(500);

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

            await _bluetoothAdapter.SendData(DmgCmd.Get("InitializeMeasurement"));
            //await Task.Delay(Constants.LongDelay);
            return measurement;
        }

        private async Task GetDgm4kB()
        {

            UpdateProgress(_progress, "Read GetDgm4kB");
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
                //await Task.Delay(Constants.LongDelay);

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
                //await Task.Delay(1000);
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

        private async Task GetDgm4kB_2()
        {
            byte[] message = { };
            _dynContainer = new DynStructuredContainer();
            var addresses = _dynContainer.GetEmptyAddresses();

            float sep_cost = (100f - _progress) / addresses.Count;

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

                message = await _bluetoothAdapter.Exchange(command.ToArray());
                if(0!= message.Length)
                {
                    var commandName = Ddim2.Ddim2Parser.DefineCommand(message);
                    MeasurementRecieveHandler(commandName, message);
                    byte[] address = new byte[] { message[4], message[5], message[6], message[7] };
                    byte[] data = Ddim2.Ddim2Parser.GetPayload(message);
                    _dynContainer.AddData(address, data);
                    _progress += sep_cost;
                    UpdateProgress(_progress, "Read GetDgm4kB");

                }

                //await Task.Delay(Constants.LongDelay);

                RemoveCrc();

                for (int i = 1; i < addresses.Count; i++)
                {
                    command[4] = addresses[i][0];
                    command[5] = addresses[i][1];

                    AddCrc();
                    message = await _bluetoothAdapter.Exchange(command.ToArray());
                    if (0 != message.Length)
                    {
                        var commandName = Ddim2.Ddim2Parser.DefineCommand(message);
                        MeasurementRecieveHandler(commandName, message);
                        byte[] address = new byte[] { message[4], message[5], message[6], message[7] };
                        byte[] data = Ddim2.Ddim2Parser.GetPayload(message);
                        _dynContainer.AddData(address, data);
                        _progress += sep_cost;
                        UpdateProgress(_progress, "Read GetDgm4kB");

                    }

                    RemoveCrc();
                }
                //await Task.Delay(1000);
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



        public void MemoryRecieveHandler(byte[] address, byte[] data)
        {
            if (_dynContainer.AddData(address, data))
            {
                //_progress += 0.5f;
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
                    var report = new List<UInt16>();
                    for (int i = 0; i + 1 < data.Count(); i += 2)
                    {
                        var array = new byte[] { data[i], data[i + 1] };
                        UInt16 value = BitConverter.ToUInt16(array, 0);
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
