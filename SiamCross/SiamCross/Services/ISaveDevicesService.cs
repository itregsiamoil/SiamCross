using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public interface ISaveDevicesService
    {
        Task SaveDevices(IEnumerable<ScannedDeviceInfo> devices);

        Task<IEnumerable<ScannedDeviceInfo>> LoadDevices();
    }
}
