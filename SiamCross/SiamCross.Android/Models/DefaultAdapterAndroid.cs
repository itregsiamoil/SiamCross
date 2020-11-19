using SiamCross.Models.Adapters;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using SiamCross.Models;
using SiamCross.Models.Scanners;

namespace SiamCross.Droid.Models
{
    public class DefaultAdapterAndroid : IDefaultAdapter
    {
        public string Name { get => "DefaultAdapter"; }

        public void Enable()
        {
            BluetoothAdapter.DefaultAdapter.Enable();
        }

        public void Disable()
        {
            BluetoothAdapter.DefaultAdapter.Disable();
        }

        public IProtocolConnection MakeConnection(ScannedDeviceInfo deviceInfo)
        {
            throw new System.NotImplementedException();
        }

        public bool IsEnbaled { get => BluetoothAdapter.DefaultAdapter.IsEnabled; }
    }
}