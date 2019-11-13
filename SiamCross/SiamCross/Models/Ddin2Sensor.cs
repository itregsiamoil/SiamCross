using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SiamCross.Models
{
    public class Ddin2Sensor : ISensor
    {
        public IBluetoothAdapter BluetoothAdapter
        {
            get;
        }

        public Ddin2Sensor(ScannedDeviceInfo deviceInfo)
        {
            BluetoothAdapter = DependencyService.Get<IBluetoothClassicAdapter>();
        }

        public bool Alive => throw new NotImplementedException();

        public SensorData SensorData => throw new NotImplementedException();

        public void QuickReport()
        {
            throw new NotImplementedException();
        }

        public void StartMeasurement()
        {
            throw new NotImplementedException();
        }
    }
}
