using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;

namespace SiamCross.Models
{
    public interface ISensor : IDisposable
    {
        IBluetoothAdapter BluetoothAdapter { get; }
        bool IsAlive { get; }

        Task QuickReport();
        Task StartMeasurement(object measurementParameters);
        SensorData SensorData { get; }
        ScannedDeviceInfo ScannedDeviceInfo { get; set; }
        event Action<SensorData> Notify;
    }
}
