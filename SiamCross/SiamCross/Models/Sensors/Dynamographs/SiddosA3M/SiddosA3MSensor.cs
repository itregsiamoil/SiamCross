using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Sensors.Dynamographs.SiddosA3M.SiddosA3MMeasurement;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dynamographs.SiddosA3M
{
    public class SiddosA3MSensor : ISensor
    {
        private CancellationTokenSource _cancellToken;
        public IBluetoothAdapter BluetoothAdapter { get; }
        public bool IsAlive { get; private set; }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        SiddosA3MMeasurementManager _measurementManager;

        public bool IsMeasurement { get; private set; }

        private SiddosA3MQuickReportBuilder _reportBuilder;
        private DynamographStatusAdapter _statusAdapter;
        private SiddosA3MParser _parser;
        private FirmWaveQualifier _firmwareQualifier;

        private Task _liveTask;
        public SiddosA3MSensor(IBluetoothAdapter adapter, SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            BluetoothAdapter = adapter;
            _firmwareQualifier = new FirmWaveQualifier(adapter.SendData);
            _parser = new SiddosA3MParser(_firmwareQualifier);
            _reportBuilder = new SiddosA3MQuickReportBuilder();
            _statusAdapter = new DynamographStatusAdapter();
            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;
            _parser.ByteMessageReceived += MeasurementRecieveHandler;

            BluetoothAdapter.ConnectSucceed += ConnectHandler;
            BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            _cancellToken = new CancellationTokenSource();
            _liveTask = new Task(() => ExecuteAsync(_cancellToken.Token));
            _liveTask.Start();
        }

        private void ConnectFailedHandler()
        {
            IsAlive = false;
        }

        private void ConnectHandler()
        {
            _firmwareQualifier = new FirmWaveQualifier(BluetoothAdapter.SendData);
            _parser = new SiddosA3MParser(_firmwareQualifier);
            _parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("СиддосА3М успешно подключен!");
        }

        private void ReceiveHandler(string commandName, string dataValue)
        {
            switch (commandName) // TODO: replace to enum 
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
                    _reportBuilder.BatteryVoltage = dataValue;
                    break;
                case "Тemperature":
                    _reportBuilder.Temperature = dataValue;
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
                default: return;
            }

            SensorData.Status = _reportBuilder.GetReport();
            //Notify?.Invoke(SensorData);
        }

        public async Task QuickReport()
        {
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["BatteryVoltage"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["Тemperature"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["LoadChanel"]);
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["AccelerationChanel"]);
        }

        public async Task CheckStatus()
        {
            await BluetoothAdapter.SendData(DynamographCommands.FullCommandDictionary["ReadDeviceStatus"]);
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(2000);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (IsAlive)
                {
                    if (SensorData.Firmware == "")
                    {
                        await _firmwareQualifier.Qualify();
                    }
                    if (!IsMeasurement)
                    {
                        await QuickReport();
                        await Task.Delay(1500);
                    }
                }
                else
                {
                    SensorData.Status = "Нет связи";

                    await BluetoothAdapter.Connect();
                    await Task.Delay(4000);
                }
            }
        }

        public async Task StartMeasurement(object measurementParameters)
        {
            IsMeasurement = true;
            _parser.MessageReceived -= ReceiveHandler;
            SiddosA3MMeasurementStartParameters specificMeasurementParameters =
                (SiddosA3MMeasurementStartParameters)measurementParameters;
            _measurementManager = new SiddosA3MMeasurementManager(BluetoothAdapter, SensorData,
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

        public void Dispose()
        {
            _cancellToken.Cancel();
            BluetoothAdapter.Disconnect();
        }

        public event Action<SensorData> Notify;
        public event Action<SiddosA3MMeasurementData> MeasurementRecieved;
    }
}
