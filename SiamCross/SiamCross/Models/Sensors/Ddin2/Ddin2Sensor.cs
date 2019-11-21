using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SiamCross.Models.Sensors.Ddin2
{
    public class Ddin2Sensor : ISensor
    {
        public IBluetoothAdapter BluetoothAdapter { get; }
        
        public SensorData SensorData { get; }

        public bool Alive { get; private set; }
        
        public Ddin2Sensor(IBluetoothAdapter bluetoothAdapter, SensorData sensorData)
        {
            Alive = false;
            BluetoothAdapter = bluetoothAdapter;
            SensorData = sensorData;
            // BluetoothAdapter = bluetoothAdapter;
        }

        public event Action<SensorData> Notify;

        public Task QuickReport()
        {
            throw new NotImplementedException();
        }

        public void StartMeasurement()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
