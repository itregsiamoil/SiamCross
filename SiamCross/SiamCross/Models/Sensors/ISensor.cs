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
        bool Activeted { get; set; }
        bool IsAlive { get; }
        bool IsMeasurement { get; }
        Task QuickReport();
        Task StartMeasurement(object measurementParameters);
        SensorData SensorData { get; }
        ScannedDeviceInfo ScannedDeviceInfo { get; set; }
    }
}
