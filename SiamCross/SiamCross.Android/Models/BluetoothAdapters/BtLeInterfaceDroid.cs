//using SiamCross.Models.Adapters;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Scanners;
using System.ComponentModel;

namespace SiamCross.Droid.Models.BluetoothAdapters
{
    public class BtLeInterfaceDroid : IBtLeInterfaceCross, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Name => "BT4LE";
        public IPhyConnection MakeConnection(ScannedDeviceInfo deviceInfo)
        {
            IPhyConnection conn = new ConnectionBtLe(deviceInfo, this);
            return conn;
        }
        public IAdapter Adapter => mBle?.Adapter;

        private IBluetoothLE mBle;

        public BtLeInterfaceDroid()
            : this(CrossBluetoothLE.Current)
        {
        }
        public BtLeInterfaceDroid(IBluetoothLE ble)
        {
            mBle = ble;
        }

        protected BtLeInterfaceDroid(bool enable = true)
        {
            if (enable)
                Enable();

        }

        public bool IsEnbaled => null != mBle
                    && null != mBle.Adapter
                    && BluetoothState.On == mBle.State;

        public void Disable()
        {
            mBle = null;
        }

        public void Enable()
        {
            mBle = CrossBluetoothLE.Current;
        }

        public IBluetoothScanner GetScanner()
        {
            return new ScannerLe(this);
        }



    }


}
