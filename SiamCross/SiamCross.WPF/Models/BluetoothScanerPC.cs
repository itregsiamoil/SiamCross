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
using InTheHand.Net.Sockets;

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

        private void FindPairedDevices()
        {
            BluetoothDeviceInfo[] devices;
            using (var client = new BluetoothClient())
            {
                int maxDevices = 255;
                bool authenticated = false;
                bool remembered = true;
                bool unknown = false;
                bool discoverableOnly = false;

                devices = client.DiscoverDevices(maxDevices,
                    authenticated, remembered, unknown, discoverableOnly);
            }
            
            foreach (var device in devices)
            {
                Received?.Invoke(new ScannedDeviceInfo(device.DeviceName, device, BluetoothType.Classic));
            }
        }

        public void Stop()
        {
            Watcher.Stop();
        }
    }
}
