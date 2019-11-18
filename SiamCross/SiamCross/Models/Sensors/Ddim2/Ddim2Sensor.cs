using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.Sensors;

namespace SiamCross.Models.Sensors.Ddim2
{
    public class Ddim2Sensor : ISensor
    {
        private static CancellationTokenSource _cancellToken =
            new CancellationTokenSource();
        public IBluetoothAdapter BluetoothAdapter { get; }
        public bool Alive { get; private set; }
        public SensorData SensorData { get; }
        private Ddim2QuickReportBuilder _reportBuilder;
        private Ddim2Parser _parser;

        private Task LiveTask;

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

            LiveTask = new Task(() => Execute(_cancellToken.Token));
            LiveTask.Start();
        }

        private void ConnectHandler()
        {
            Alive = true;
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

        public void QuickReport()
        {
            BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["BatteryVoltage"]);
            BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["Тemperature"]);
            BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["LoadChanel"]);
            BluetoothAdapter.SendData(Ddim2Commands.FullCommandDictionary["AccelerationChanel"]);
        }

        private void Execute(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                if(Alive)
                {
                    QuickReport();
                }
                else
                {
                    BluetoothAdapter.Connect();
                    Task.Delay(4000);
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
