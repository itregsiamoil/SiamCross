using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors;

namespace SiamCross.Models.Sensors.Ddim2
{
    public class Ddim2Sensor : ISensor
    {
        private CancellationTokenSource _cancellToken;
        public IBluetoothAdapter BluetoothAdapter { get; }
        public bool Alive { get; private set; }
        public SensorData SensorData { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }

        private Ddim2QuickReportBuilder _reportBuilder;
        private Ddim2Parser _parser;

        private Task _liveTask;
        public Ddim2Sensor(IBluetoothAdapter adapter, SensorData sensorData)
        {
            Alive = false;
            SensorData = sensorData;
            BluetoothAdapter = adapter;
            _parser = new Ddim2Parser();
            _reportBuilder = new Ddim2QuickReportBuilder();
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
            Alive = false;
        }

        private void ConnectHandler()
        {
            _parser = new Ddim2Parser();
            _parser.MessageReceived += ReceiveHandler;

            Alive = true;
            System.Diagnostics.Debug.WriteLine("Ддим2 успешно подключен!");
        }

        private void ReceiveHandler(string commandName, string dataValue)
        {
            switch (commandName) // TODO: replace to enum 
            {
                case "DeviceStatus":
                    SensorData.Status = dataValue;
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

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(2000);
            while(!cancellationToken.IsCancellationRequested)
            {
                if(Alive)
                {
                    await QuickReport();
                    await Task.Delay(1500);
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

        public void StartMeasurement()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            BluetoothAdapter.Disconnect();
        }

        public event Action<SensorData> Notify;
    }
}
