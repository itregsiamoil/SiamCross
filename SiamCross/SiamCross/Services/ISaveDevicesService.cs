using SiamCross.Models.Scanners;
using System.Collections.Generic;

namespace SiamCross.Services
{
    public interface ISaveDevicesService
    {
        void SaveDevices(IEnumerable<ScannedDeviceInfo> devices);

        List<ScannedDeviceInfo> LoadDevices();
    }
}
