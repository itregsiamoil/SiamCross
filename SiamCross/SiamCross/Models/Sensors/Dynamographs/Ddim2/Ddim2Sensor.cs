using System;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dynamographs.Ddim2.Measurement;
using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Tools;
using SiamCross.Services;

namespace SiamCross.Models.Sensors.Dynamographs.Ddim2
{
    public class Ddim2Sensor : ISensor
    {
        private CancellationTokenSource _cancellToken;
        public IBluetoothAdapter BluetoothAdapter { get; }
        public bool IsAlive { get; private set; }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        Ddim2MeasurementManager _measurementManager;

        public bool IsMeasurement { get; private set; } 

        private Ddim2QuickReportBuilder _reportBuilder;
        private DynamographStatusAdapter _statusAdapter;
        private Ddim2Parser _parser;
        private FirmWaveQualifier _firmwareQualifier;

        private Task _liveTask;
        public Ddim2Sensor(IBluetoothAdapter adapter, SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            BluetoothAdapter = adapter;
            _firmwareQualifier = new FirmWaveQualifier(
                adapter.SendData,
                DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"],
                DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]
            );
            _parser = new Ddim2Parser(_firmwareQualifier, true);
            _reportBuilder = new Ddim2QuickReportBuilder();
            _statusAdapter = new DynamographStatusAdapter();

            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementRecieveHandler;
            _parser.ExportMemoryFragment += MemoryRecieveHandler;

            BluetoothAdapter.ConnectSucceed += ConnectHandler;
            BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            _cancellToken = new CancellationTokenSource();
            _liveTask = new Task(async () => await ExecuteAsync(_cancellToken.Token));
            _liveTask.Start();
        }

        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            _firmwareQualifier = new FirmWaveQualifier(
                BluetoothAdapter.SendData,
                DynamographCommands.FullCommandDictionary["ProgrammVersionAddress"],
                DynamographCommands.FullCommandDictionary["ProgrammVersionSize"]
            );
            _parser = new Ddim2Parser(_firmwareQualifier, true);
            _parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("Ддим2 успешно подключен!");
            SensorData.Status = Resource.ConnectedStatus;
        }

        private void ReceiveHandler(string dataName, string dataValue)
        {
            switch (dataName)
            {
                case "DeviceStatus":
                    /*/ Для замера /*/
                    if (_measurementManager != null)
                    {
                        _measurementManager.MeasurementStatus = _statusAdapter.StringStatusToEnum(dataValue);
                    }
                    SensorData.Status = _statusAdapter.StringStatusToReport(dataValue);
                    return;
                case "BatteryVoltage":
                    //_reportBuilder.BatteryVoltage = dataValue;
                    SensorData.Battery = dataValue;
                    break;
                case "Тemperature":
                    //_reportBuilder.Temperature = dataValue;
                    SensorData.Temperature = dataValue;
                    break;
                case "LoadChanel":
                    _reportBuilder.Load = dataValue;
                    break;
                case "AccelerationChanel":
                    _reportBuilder.Acceleration = dataValue;
                    break;
                case "DeviceProgrammVersion":
                    SensorData.Firmware = dataValue;
                    return;
                case "SensorLoadRKP":
                    _reportBuilder.SensitivityLoad = dataValue;
                    return;
                case "SensorLoadNKP":
                    _reportBuilder.ZeroOffsetLoad = dataValue;
                    return;
                default: return;             
            }

            SensorData.Status = _reportBuilder.GetReport();
        }

        public async Task QuickReport()
        {
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["BatteryVoltage"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["Тemperature"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["LoadChanel"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["AccelerationChanel"]);
        }

        public async Task KillosParametersQuery()
        {
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["SensorLoadRKP"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["SensorLoadNKP"]);
        }

        public async Task CheckStatus()
        {
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]);
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(2000);
            while(!cancellationToken.IsCancellationRequested)
            {
                if(IsAlive)
                {
                    if(SensorData.Firmware == "")
                    {
                        await _firmwareQualifier.Qualify();
                    }
                    if (!_reportBuilder.IsKillosParametersReady)
                    {
                        await KillosParametersQuery();
                    }
                    if (!IsMeasurement)
                    {
                        await QuickReport();
                        await Task.Delay(1500);
                    }
                }
                else
                {
                    SensorData.Status = Resource.NoConnection;

                    await BluetoothAdapter.Connect();
                    await Task.Delay(1000);
                }
            }
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            IsMeasurement = true;
            _parser.MessageReceived -= ReceiveHandler;
            Ddim2MeasurementStartParameters specificMeasurementParameters = 
                (Ddim2MeasurementStartParameters)measurementParameters;
            _measurementManager = new Ddim2MeasurementManager(BluetoothAdapter, SensorData, 
                specificMeasurementParameters);
            var report = await _measurementManager.RunMeasurement();
            SensorService.Instance.MeasurementHandler(report);
            IsMeasurement = false;
        }

        /*/ Для замера /*/
        private void MeasurementRecieveHandler(string commandName, byte[] data)
        {
            _measurementManager.MeasurementRecieveHandler(commandName, data);
        }

        private void MemoryRecieveHandler(byte[] address, byte[] data)
        {
            if (_measurementManager != null)
            {
                _measurementManager.MemoryRecieveHandler(address, data);
                SensorData.Status = _statusAdapter.CreateProgressStatus(_measurementManager.Progress);             
            }
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            BluetoothAdapter.Disconnect();
        }
    }
}
