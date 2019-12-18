using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Ddim2.Measurement;

namespace SiamCross.Models.Sensors.Ddim2
{
    public class Ddim2Sensor : ISensor
    {
        private CancellationTokenSource _cancellToken;
        public IBluetoothAdapter BluetoothAdapter { get; }
        public bool IsAlive { get; private set; }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        private Ddim2StatusAdapter _statusAdapter;
        Ddim2MeasurementManager _measurementManager;

        private bool IsMeasurement { get; set; } 

        private Ddim2QuickReportBuilder _reportBuilder;
        private Ddim2Parser _parser;

        private Task _liveTask;
        public Ddim2Sensor(IBluetoothAdapter adapter, SensorData sensorData)
        {
            IsMeasurement = false;
            IsAlive = false;
            SensorData = sensorData;
            BluetoothAdapter = adapter;
            _parser = new Ddim2Parser();
            _reportBuilder = new Ddim2QuickReportBuilder();
            _statusAdapter = new Ddim2StatusAdapter();
            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += ReceiveHandler;

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
            _parser = new Ddim2Parser();
            _parser.MessageReceived += ReceiveHandler;

            IsAlive = true;
            System.Diagnostics.Debug.WriteLine("Ддим2 успешно подключен!");
        }

        private async void ReceiveHandler(string commandName, string dataValue)
        {
            switch (commandName) // TODO: replace to enum 
            {
                case "DeviceStatus":
                    SensorData.Status = _statusAdapter.StringStatusToReport(dataValue);
                    if(_statusAdapter.StringStatusToEnum(dataValue) == Ddim2MeasurementStatusState.Ready)
                    {
                        var measurement = await _measurementManager.DownloadMeasurement();                       
                        MeasurementRecieved(measurement);
                    }
                    else if(_statusAdapter.StringStatusToEnum(dataValue) == Ddim2MeasurementStatusState.Empty)
                    {
                        IsMeasurement = false;
                    }
                    break;
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
                default: return;             
            }

            SensorData.Status = _reportBuilder.GetReport();
            Notify?.Invoke(SensorData);
        }

        public async Task QuickReport()
        {
            await BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["BatteryVoltage"]);
            await BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["Тemperature"]);
            await BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["LoadChanel"]);
            await BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["AccelerationChanel"]);
        }

        public async Task CheckStatus()
        {
            await BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["ReadDeviceStatus"]);
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(2000);
            while(!cancellationToken.IsCancellationRequested)
            {
                if(IsAlive)
                {
                    if(!IsMeasurement)
                    {
                        await QuickReport();
                        await Task.Delay(1500);
                    }
                    else
                    {
                        await CheckStatus();
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    SensorData.Status = "Нет связи";
                    Notify?.Invoke(SensorData);

                    await BluetoothAdapter.Connect();
                    await Task.Delay(4000);
                }
            }
        }

        public async Task StartMeasurement(object measurementParameters,
            object secondaryParameters)
        {
            IsMeasurement = true;
            Ddim2MeasurementParameters specificMeasurementParameters = 
                (Ddim2MeasurementParameters)measurementParameters;
            Ddim2SecondaryParameters specificSecondaryParameters = 
                (Ddim2SecondaryParameters)secondaryParameters;
            _measurementManager = new Ddim2MeasurementManager(
                BluetoothAdapter,
                specificMeasurementParameters,
                specificSecondaryParameters);
            await _measurementManager.RunMeasurement();
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            BluetoothAdapter.Disconnect();
        }

        public event Action<SensorData> Notify;
        public event Action<List<Ddim2MeasurementData>> MeasurementRecieved;
    }
}
