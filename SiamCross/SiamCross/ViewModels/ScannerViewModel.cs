using SiamCross.Models.Scanners;
using SiamCross.Services;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Windows.Input;

namespace SiamCross.ViewModels
{
    public class ScannerViewModel : BaseViewModel
    {
        private readonly IScannedDevicesService _service;

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ObservableCollection<ScannedDeviceInfo> ClassicDevices { get; }

        public ICommand Connect { get; private set; }

        public ICommand SendMessage { get; private set; }

        public ICommand Disconnect { get; private set; }

        public ScannedDeviceInfo SelectedDevice { get; set;} 
                
        public ScannerViewModel()
        {
            _service = DependencyService.Resolve<IScannedDevicesService>();
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            ClassicDevices = new ObservableCollection<ScannedDeviceInfo>();

            Connect = new Command(
                execute: () => 
                {
                    
                });

            SendMessage = new Command(
                execute: () =>
                {

                });

            Disconnect = new Command(
                execute: () =>
                {

                });


            _service.PropertyChanged += ServicePropertyChanged;
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
