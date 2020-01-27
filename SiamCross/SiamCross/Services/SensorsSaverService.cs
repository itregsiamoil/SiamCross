using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public sealed class SensorsSaverService
    {
        #region instance
        private static readonly
            Lazy<SensorsSaverService> _instance =
            new Lazy<SensorsSaverService>(() => new SensorsSaverService());

        public static SensorsSaverService Instance { get => _instance.Value; }
        #endregion

        private readonly ISaveDevicesService _manager;
        private SensorsSaverService()
        {
            _manager = DependencyService.Get<ISaveDevicesService>();
            MessagingCenter.Subscribe<SensorService, IEnumerable<ScannedDeviceInfo>>(this,
                "Refresh saved sensors", 
                async (sender, arg) => 
                {
                    await _manager.SaveDevices(arg);
                });
        }

        public async Task<IEnumerable<ScannedDeviceInfo>> ReadSavedSensors()
        {
            var sensors = await _manager.LoadDevices();

            return sensors;
        }
    }
}
