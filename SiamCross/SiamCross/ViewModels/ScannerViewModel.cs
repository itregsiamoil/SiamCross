using SiamCross.Models.Scanners;
using SiamCross.Services;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class ScannerViewModel : BaseViewModel
    {
        private readonly IScannedDevicesService _service;

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ScannerViewModel()
        {
            _service = DependencyService.Resolve<IScannedDevicesService>();
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            _service.PropertyChanged += ServicePropertyChanged;
            _service.StartScan();
        }

        private void ServicePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ScannedDevices.Clear();
                foreach (var deviceInfo in _service.ScannedDevices)
                {
                    ScannedDevices.Add(deviceInfo);
                }
            });
        }
    }
}
