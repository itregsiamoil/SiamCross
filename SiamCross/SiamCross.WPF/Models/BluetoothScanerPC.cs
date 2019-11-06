using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using SiamCross.Models;
using Xamarin.Forms;

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
            Received?.Invoke(args.Advertisement.LocalName, args);
        }

        public event Action<string, object> Received;

        public void Start()
        {
            Watcher.Start();
        }

        public void Stop()
        {
            Watcher.Stop();
        }
    }
}
