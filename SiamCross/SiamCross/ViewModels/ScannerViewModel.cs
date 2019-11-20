using SiamCross.Models.Scanners;
using SiamCross.Services;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Windows.Input;
using SiamCross.Models;
using MvvmCross.Plugin.Messenger;

namespace SiamCross.ViewModels
{
    public class ScannerViewModel : IViewModel
    {
        private readonly IScannedDevicesService _service;
        private readonly IMvxMessenger _messenger;

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ObservableCollection<ScannedDeviceInfo> ClassicDevices { get; }

        public ICommand Connect { get; private set; }

        public ICommand SendMessage { get; private set; }

        public ICommand Disconnect { get; private set; }

        public ScannedDeviceInfo SelectedDevice { get; set;}

        private ISensor _sensor;

        public event PropertyChangedEventHandler PropertyChanged;

        public ScannerViewModel(IScannedDevicesService service, IMvxMessenger messenger)
        {
            _service = service;
            _messenger = messenger;
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            ClassicDevices = new ObservableCollection<ScannedDeviceInfo>();

            Connect = new Command(
                execute: async () => 
                {
                    if (SelectedDevice != null)
                    {
                        _sensor = SensorFactory.CreateSensor(SelectedDevice);
                        if (_sensor != null)
                        {
                            await _sensor.BluetoothAdapter.Connect();
                        }
                    }
                });

            SendMessage = new Command(
                execute: async () =>
                {
                    var message = new byte[]
                    {
                        0x0D, 0x0A,
                        0x01, 0x01,
                        0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
                        0x90, 0x67
                    };
                    await _sensor.BluetoothAdapter.SendData(message);
                });

            Disconnect = new Command(
                execute: async () =>
                {
                    await _sensor.BluetoothAdapter.Disconnect();
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
