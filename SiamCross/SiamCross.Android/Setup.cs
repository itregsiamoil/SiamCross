using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Autofac;
using SiamCross.AppObjects;
using SiamCross.Droid.Models;
using SiamCross.Droid.Services;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Services;

namespace SiamCross.Droid
{
    public class Setup : AppSetup
    {
        protected override void RegisterDependencies(ContainerBuilder cb)
        {
            cb.RegisterType<BluetoothScannerMobile>().As<IBluetoothScanner>();
            base.RegisterDependencies(cb);

            cb.RegisterType<BluetoothClassicAdapterMobile>().As<IBluetoothClassicAdapter>();
            cb.RegisterType<BluetoothLeAdapterMobile>().As<IBluetoothLeAdapter>();
            cb.RegisterType<SaveDevicesServiceMobile>().As<ISaveDevicesService>();
        }
    }    
}