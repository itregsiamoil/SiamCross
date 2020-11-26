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
        IProtocolConnection Connection { get; }
        bool Activate { get; set; }
        bool IsAlive { get; }
        bool IsMeasurement { get; }
        float MeasureProgress { get; set; }
        Task<bool> QuickReport();
        Task StartMeasurement(object measurementParameters);
        SensorData SensorData { get; }
        ScannedDeviceInfo ScannedDeviceInfo { get; set; }
    }
}
