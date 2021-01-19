using Android.Bluetooth;
using Android.Runtime;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;

namespace SiamCross.Droid.Models
{
    [Preserve(AllMembers = true)]
    public class DefaultAdapterAndroid : IDefaultAdapter
    {
        public string Name => "DefaultAdapter";

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

        public bool IsEnbaled => BluetoothAdapter.DefaultAdapter.IsEnabled;
    }
}