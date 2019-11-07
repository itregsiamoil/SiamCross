using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Services
{
    public interface IScannedDevicesService
    {
        IEnumerable<ScannedDeviceInfo> GetScannedDevices();
    }
}
