using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public interface ISaveDevicesService
    {
        void SaveDevices(IEnumerable<ScannedDeviceInfo> devices);

        List<ScannedDeviceInfo> LoadDevices();
    }
}
