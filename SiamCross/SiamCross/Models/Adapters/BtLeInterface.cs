using System;
using System.Collections.Generic;
using System.Text;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using SiamCross.Models.Adapters;

//using SiamCross.Models.Adapters;
using SiamCross.Droid.Models;
using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Scanners;

namespace SiamCross.Models.Adapters
{
    public class BtLeInterface: IPhyInterface
    {
        public IConnection MakeConnection(ScannedDeviceInfo deviceInfo)
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
