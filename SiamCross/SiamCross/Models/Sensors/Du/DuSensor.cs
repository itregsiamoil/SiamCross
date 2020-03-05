using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Du
{
    public class DuSensor : ISensor
    {
        public IBluetoothAdapter BluetoothAdapter => throw new NotImplementedException();

        public bool IsAlive => throw new NotImplementedException();

        public bool IsMeasurement => throw new NotImplementedException();

        public SensorData SensorData => throw new NotImplementedException();

        public ScannedDeviceInfo ScannedDeviceInfo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event Action<SensorData> Notify;

        public void Dispose()
        {
            throw new NotImplementedException();
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
