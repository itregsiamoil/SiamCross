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
        bool Alive { get; }

        event Action<SensorData> Notify;

        Task QuickReport();
        void StartMeasurement();
        SensorData SensorData { get; }

        ScannedDeviceInfo ScannedDeviceInfo { get; set; }
    }
}
