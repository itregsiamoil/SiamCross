using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using SiamCross.Models;
using Xamarin.Forms;
using SiamCross.Models.Scanners;
using Windows.Devices.Enumeration;

[assembly: Dependency(typeof(SiamCross.WPF.Models.BluetoothScanerPC))]
namespace SiamCross.WPF.Models
{
    public class BluetoothScanerPC : IBluetoothScanner
    {
        public BluetoothLEAdvertisementWatcher Watcher { get; set; }

        public BluetoothScanerPC()
        {
            Watcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            Watcher.Received += OnRecieved;
            Watcher.Stopped += WatcherStopped;
            Watcher.Start();
        }

        private void WatcherStopped(BluetoothLEAdvertisementWatcher sender,
            BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void OnRecieved(BluetoothLEAdvertisementWatcher sender,
            BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Received?.Invoke(new ScannedDeviceInfo(args.Advertisement.LocalName, args, BluetoothType.Le));
        }

        public event Action<ScannedDeviceInfo> Received;

        public void Start()
        {
            FindPairedDevices();
            Watcher.Start();
        }

        private async Task FindPairedDevices()
        {
            DeviceInformationCollection pairedDevices =
                await DeviceInformation.FindAllAsync(
                    BluetoothDevice.GetDeviceSelectorFromPairingState(true));
            foreach (var device in pairedDevices)
            {
                //if (device.IsEnabled)
                //{
                    Received?.Invoke(new ScannedDeviceInfo(device.Name, device, BluetoothType.Classic));
                //}
            }
        }

        public void Stop()
        {
            Watcher.Stop();
        }
    }
}
