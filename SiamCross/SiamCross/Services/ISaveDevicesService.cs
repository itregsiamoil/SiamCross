using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public interface ISaveDevicesService
    {
        Task SaveDevices(IEnumerable<ScannedDeviceInfo> devices);

        Task<IEnumerable<ScannedDeviceInfo>> LoadDevices();
    }
}
