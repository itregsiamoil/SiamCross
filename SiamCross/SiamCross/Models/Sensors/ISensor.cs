using SiamCross.Models.Scanners;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models
{
    public interface ISensor : IDisposable
    {
        IProtocolConnection Connection { get; }

        //Task ActivateAsync();
        //Task DeactivateAsync();

        string ConnStateStr { get; }
        bool Activate { get; set; }
        bool IsAlive { get; }
        bool IsMeasurement { get; }
        float MeasureProgress { get; set; }
        Task<bool> QuickReport(CancellationToken cancelToken);
        Task StartMeasurement(object measurementParameters);
        SensorData SensorData { get; }
        ScannedDeviceInfo ScannedDeviceInfo { get; set; }
    }
}
