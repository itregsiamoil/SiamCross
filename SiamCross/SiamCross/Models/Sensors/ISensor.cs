using System;
using System.Collections.Generic;
using System.Text;
using SiamCross.Models.Tools;

namespace SiamCross.Models
{
    public interface ISensor : IDisposable
    {
        IBluetoothAdapter BluetoothAdapter { get; }
        bool Alive { get; }

        event Action<SensorData> Notify;

        void QuickReport();
        void StartMeasurement();
        SensorData SensorData { get; }
    }
}
