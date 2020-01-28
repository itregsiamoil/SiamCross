using SiamCross.Models.Scanners;
using SiamCross.Services;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Windows.Input;
using SiamCross.Models;

namespace SiamCross.ViewModels
{
    public class ScannerViewModel : IViewModel
    {
        private  IScannedDevicesService _service;

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ObservableCollection<ScannedDeviceInfo> ClassicDevices { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ScannerViewModel(IScannedDevicesService service)
        {
            _service = service;
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            ClassicDevices = new ObservableCollection<ScannedDeviceInfo>();

            _service.PropertyChanged += ServicePropertyChanged;
            _service.StartScan();
        }

        public void StartScan()
        {
            _service.StartScan();
        }

        private void ServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ScannedDevices.Clear();
                ClassicDevices.Clear();
                foreach (var deviceInfo in _service.ScannedDevices)
                {
                    if (deviceInfo.BluetoothType == Models.BluetoothType.Classic)
                    {
                        ClassicDevices.Add(deviceInfo);
                    }
                    else
                    {
                        ScannedDevices.Add(deviceInfo);
                    }
                }
            });
        }
    }
}
