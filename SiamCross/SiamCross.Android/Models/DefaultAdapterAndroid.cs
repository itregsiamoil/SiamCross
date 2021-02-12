using Android.Bluetooth;
using Android.Runtime;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Connection.Phy;
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

        IPhyConnection IPhyInterface.MakeConnection(ScannedDeviceInfo deviceInfo)
        {
            /*
            switch (deviceInfo.BluetoothType)
            {
                default: break;
                case BluetoothType.Le:
                    return new BaseBluetoothLeAdapterAndroid(deviceInfo, this);
                case BluetoothType.Classic:
                    return new BaseBluetoothClassicAdapterAndroid(deviceInfo, this);
            }
            */
            return null;
        }

        public bool IsEnbaled => BluetoothAdapter.DefaultAdapter.IsEnabled;
    }
}