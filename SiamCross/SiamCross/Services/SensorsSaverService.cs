using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SiamCross.Services
{
    public sealed class SensorsSaverService
    {
        #region instance
        private static readonly
            Lazy<SensorsSaverService> _instance =
            new Lazy<SensorsSaverService>(() => new SensorsSaverService());

        public static SensorsSaverService Instance => _instance.Value;
        #endregion

        private readonly ISaveDevicesService _manager;
        private SensorsSaverService()
        {
            _manager = DependencyService.Get<ISaveDevicesService>();
            MessagingCenter.Subscribe<SensorService, IEnumerable<ScannedDeviceInfo>>(this,
                "Refresh saved sensors",
                (sender, arg) =>
                {
                    _manager.SaveDevices(arg);
                });
        }

        public IEnumerable<ScannedDeviceInfo> ReadSavedSensors()
        {
            List<ScannedDeviceInfo> sensors = _manager.LoadDevices();

            return sensors;
        }
    }
}
