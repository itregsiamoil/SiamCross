using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SiamCross.Models.Scanners;

namespace SiamCross.Droid.Models
{
    public class BLEScanner : IBluetoothScanner
    {
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothLeScanner _scanner;
        private BluetoothAdapter.ILeScanCallback _scanCallback;

        public BLEScanner()
        {

            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            _scanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;
            _scanCallback = new ScanCallback();
        }

        public event Action<ScannedDeviceInfo> Received;

        public void Start()
        {
            _scanner.StartScan(_scanCallback);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }

    public class BluetoothDeviceReceiver : BroadcastReceiver
    {

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;

            if (action == BluetoothDevice.ActionFound)
            {
                // Get device
                BluetoothDevice newDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                // now you could do your job with newDevice
                // etc. check if newDevice is not already in a list and then use it in a ListView
            }
        }
    }
}