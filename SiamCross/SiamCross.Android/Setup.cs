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
using SiamCross.Models.Adapters;

namespace SiamCross.Droid
{
    public class Setup : AppSetup
    {
        protected override void RegisterDependencies(ContainerBuilder cb)
        {
            base.RegisterDependencies(cb);

            cb.RegisterType<BluetoothClassicAdapterMobile>().As<IBluetoothClassicAdapter>();
            cb.RegisterType<BluetoothLeAdapterMobile>().As<IBluetoothLeAdapter>();
        }
    }    
}