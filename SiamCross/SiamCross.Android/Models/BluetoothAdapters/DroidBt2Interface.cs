using Android.Bluetooth;
using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Scanners;
using System.ComponentModel;

namespace SiamCross.Models.Adapters.PhyInterface.Bt2
{
    public class DroidBt2Interface : IBt2InterfaceCross, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Name { get => "BT2"; }
        public IProtocolConnection MakeConnection(ScannedDeviceInfo deviceInfo)
        {
            var t_dvc_inf = new TypedParameter(typeof(ScannedDeviceInfo), deviceInfo);
            var t_phy_ifc = new TypedParameter(typeof(IPhyInterface), this);
            var connection = AppContainer.Container.Resolve<IConnectionBt2>(t_dvc_inf, t_phy_ifc);
            return connection;
        }
        public BluetoothAdapter Adapter
        {
            get
            {
                return mBt2;
            }
        }


        private BluetoothAdapter mBt2;

        public DroidBt2Interface()
            :this(BluetoothAdapter.DefaultAdapter)
        {
        }

        protected DroidBt2Interface(BluetoothAdapter adapter)
        {
            mBt2 = adapter;
        }

        protected DroidBt2Interface(bool enable = true)
        {
            if (enable)
                Enable();

        }

        public bool IsEnbaled
        {
            get
            {
                return null != mBt2
                    && Android.Bluetooth.State.On == mBt2.State;
            }
        }

        public void Disable()
        {
            mBt2 = null;
        }

        public void Enable()
        {
            mBt2 = BluetoothAdapter.DefaultAdapter;
        }

    }
}
