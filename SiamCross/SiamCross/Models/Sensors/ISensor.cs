using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    }
}
