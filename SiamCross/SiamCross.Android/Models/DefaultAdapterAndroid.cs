using SiamCross.Models.Adapters;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;


namespace SiamCross.Droid.Models
{
    public class DefaultAdapterAndroid : IDefaultAdapter
    {
        public void Enable()
        {
            BluetoothAdapter.DefaultAdapter.Enable();
        }

        public void Disable()
        {
            BluetoothAdapter.DefaultAdapter.Disable();
        }

        public bool IsEnbaled { get => BluetoothAdapter.DefaultAdapter.IsEnabled; }
    }
}