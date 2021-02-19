using Android.Bluetooth;
using Android.Runtime;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Scanners;
using System.ComponentModel;

namespace SiamCross.Droid.Models.BluetoothAdapters
{
    [Preserve(AllMembers = true)]
    public class Bt2InterfaceDroid : IBt2InterfaceCross, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Name => "BT2";
        public IPhyConnection MakeConnection(ScannedDeviceInfo deviceInfo)
        {
            return new ConnectionBt2(deviceInfo, this);
        }
        public BluetoothAdapter Adapter => mBt2;


        private BluetoothAdapter mBt2;

        public Bt2InterfaceDroid()
            : this(BluetoothAdapter.DefaultAdapter)
        {
        }

        protected Bt2InterfaceDroid(BluetoothAdapter adapter)
        {
            mBt2 = adapter;
        }

        protected Bt2InterfaceDroid(bool enable = true)
        {
            if (enable)
                Enable();

        }

        public bool IsEnbaled => null != mBt2
                    && State.On == mBt2.State;

        public void Disable()
        {
            mBt2 = null;
        }

        public void Enable()
        {
            mBt2 = BluetoothAdapter.DefaultAdapter;
        }

        public IBluetoothScanner GetScanner()
        {
            return new ScannerBt2(this);
        }
    }
}
