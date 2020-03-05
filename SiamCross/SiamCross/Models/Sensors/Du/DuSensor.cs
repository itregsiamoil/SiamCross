using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du
{
    public class DuSensor : ISensor
    {
        public IBluetoothAdapter BluetoothAdapter { get; }
        public ScannedDeviceInfo ScannedDeviceInfo { get; set; }


        public event Action<SensorData> Notify;

        public SensorData SensorData { get; }
        public bool IsMeasurement { get; }
        public bool IsAlive { get; }
        private Task _liveTask;
        private CancellationTokenSource _cancelSource =
            new CancellationTokenSource();

        public DuSensor(IBluetoothAdapter adapter, 
                        SensorData sensorData)
        {
            BluetoothAdapter = adapter;
            SensorData = sensorData;

            IsMeasurement = false;
            IsAlive = false;
            _liveTask = new Task(async () => await LiveWhile(_cancelSource.Token));
            _liveTask.Start();
        }

        private async Task LiveWhile(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (IsAlive)
                {
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
                    await Task.Delay(4000);
                }
            }
        }

        public void Dispose()
        {
            _cancelSource.Cancel();
            BluetoothAdapter.Disconnect();
        }

        public Task QuickReport()
        {
            throw new NotImplementedException();
        }

        public Task StartMeasurement(object measurementParameters)
        {
            throw new NotImplementedException();
        }
    }
}
