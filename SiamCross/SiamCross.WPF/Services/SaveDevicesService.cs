using SiamCross.Models.Scanners;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.WPF.Services
{
    public class SaveDevicesService : ISaveDevicesService
    {
        public Task<IEnumerable<ScannedDeviceInfo>> LoadDevices()
        {
            throw new NotImplementedException();
        }

        public Task SaveDevices(IEnumerable<ScannedDeviceInfo> devices)
        {
            throw new NotImplementedException();
        }
    }
}
