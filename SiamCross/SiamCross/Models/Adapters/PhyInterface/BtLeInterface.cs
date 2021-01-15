//using SiamCross.Models.Adapters;
using Autofac;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using SiamCross.AppObjects;
using SiamCross.Models.Scanners;
using System.ComponentModel;

namespace SiamCross.Models.Adapters
{
    public class BtLeInterface: IPhyInterface, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Name { get => "BT4LE"; }
        public IProtocolConnection MakeConnection(ScannedDeviceInfo deviceInfo)
        {
            var t_dvc_inf = new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo);
            var t_phy_ifc = new TypedParameter(typeof(IPhyInterface), this);
            var connection = AppContainer.Container.Resolve<IConnectionBtLe>(t_dvc_inf, t_phy_ifc);
            return connection;
        }
        public IAdapter mAdapter
        {
            get 
            {
                return mBle?.Adapter;
            }
        }

        private IBluetoothLE mBle;

        protected BtLeInterface(IBluetoothLE ble)
        {
            mBle = ble;
        }

        protected BtLeInterface(bool enable=true)
        {
            if (enable)
                Enable();

        }

        public bool IsEnbaled 
        { 
            get
            {
                return null != mBle
                    && null != mBle.Adapter 
                    && BluetoothState.On == mBle.State;
            }
        }

        public void Disable()
        {
            mBle = null;
        }

        public void Enable()
        {
            mBle = CrossBluetoothLE.Current;
        }

        static public class Factory
        {
            static public BtLeInterface GetCurent()
            {
                return new BtLeInterface(CrossBluetoothLE.Current);
            }
        }//static public class Factory

    }


}
