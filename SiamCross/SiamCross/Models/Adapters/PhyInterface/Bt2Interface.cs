using Android.Bluetooth;
using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SiamCross.Models.Adapters.PhyInterface
{
    public class Bt2Interface : IPhyInterface, INotifyPropertyChanged
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
        public BluetoothAdapter mAdapter
        {
            get
            {
                return mBt2;
            }
        }


        private BluetoothAdapter mBt2;

        protected Bt2Interface(BluetoothAdapter adapter)
        {
            mBt2 = adapter;
        }

        protected Bt2Interface(bool enable = true)
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

        static public class Factory
        {
            static public Bt2Interface GetCurent()
            {
                return new Bt2Interface(BluetoothAdapter.DefaultAdapter);
            }
        }//static public class Factory

    }
}
