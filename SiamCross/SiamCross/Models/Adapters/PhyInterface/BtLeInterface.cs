//using SiamCross.Models.Adapters;
using Autofac;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using SiamCross.AppObjects;
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Scanners;
using System.ComponentModel;

namespace SiamCross.Models.Adapters
{
    public class BtLeInterface : IPhyInterface, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Name => "BT4LE";
        public IPhyConnection MakeConnection(ScannedDeviceInfo deviceInfo)
        {
            TypedParameter t_dvc_inf = new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo);
            TypedParameter t_phy_ifc = new TypedParameter(typeof(IPhyInterface), this);
            IConnectionBtLe connection = AppContainer.Container.Resolve<IConnectionBtLe>(t_dvc_inf, t_phy_ifc);
            return connection;
        }
        public IAdapter Adapter => mBle?.Adapter;

        private IBluetoothLE mBle;

        protected BtLeInterface(IBluetoothLE ble)
        {
            mBle = ble;
        }

        protected BtLeInterface(bool enable = true)
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

        public static class Factory
        {
            public static BtLeInterface GetCurent()
            {
                return new BtLeInterface(CrossBluetoothLE.Current);
            }
        }//static public class Factory

    }


}
