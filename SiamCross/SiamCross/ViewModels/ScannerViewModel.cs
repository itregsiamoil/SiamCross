using MvvmCross.Plugin.Messenger;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.ViewModels
{
    public class ScannerViewModel : BaseViewModel
    {
        private readonly IScannedDevicesService _service;

        private readonly MvxSubscriptionToken _token;

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ScannerViewModel(IScannedDevicesService service, 
                                IMvxMessenger messenger)
        {
            _service = service;
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            _token =
                messenger.SubscribeOnMainThread<ScannedDevicesListChangedMessage>
                ( m => RefreshDevicesList());
            _service.StartScan();
        }

        public void RefreshDevicesList()
        {
            ScannedDevices.Clear();
            foreach (var deviceInfo in _service.GetScannedDevices())
            {
                ScannedDevices.Add(deviceInfo);
            }
        }
    }
}
