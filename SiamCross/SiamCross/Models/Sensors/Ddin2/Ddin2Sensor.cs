using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2Sensor : ISensor
    {
        private Ddin2Parser _parser;
        private Ddin2QuickReportBuiler _reportBuilder;
        private Task _liveTask;
        private CancellationTokenSource _cancellToken =
            new CancellationTokenSource();

        public IBluetoothAdapter BluetoothAdapter { get; }
        
        public SensorData SensorData { get; }

        public bool Alive { get; private set; }

        public event Action<SensorData> Notify;

        public Ddin2Sensor(IBluetoothAdapter bluetoothAdapter, SensorData sensorData)
        {
            Alive = false;
            BluetoothAdapter = bluetoothAdapter;
            SensorData = sensorData;
            _parser = new Ddin2Parser();
            _reportBuilder = new Ddin2QuickReportBuiler();

            BluetoothAdapter.DataReceived += _parser.ByteProcess;
            _parser.MessageReceived += MessageReceivedHandler;
            BluetoothAdapter.ConnectSucceed += ConnectSucceedHandler;
            BluetoothAdapter.ConnectFailed += ConnectFailedHandler;

            _liveTask = new Task( async () => await LiveWhileAsync(_cancellToken.Token));
            _liveTask.Start();
        }

        private void ConnectFailedHandler()
        {
            //Alive = false;
        }

        private void ConnectSucceedHandler()
        {
            Alive = true;
        }

        private async Task LiveWhileAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Alive)
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

        public async Task QuickReport()
        {
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["BatteryVoltage"]);
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["Тemperature"]);
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["LoadChanel"]);
            await BluetoothAdapter.SendData(Ddin2Commands.FullCommandDictionary["AccelerationChanel"]);
        }

        private void MessageReceivedHandler(string commandName, string dataValue)
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


        public void StartMeasurement()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _cancellToken.Cancel();
            BluetoothAdapter.Disconnect();
        }
    }
}
