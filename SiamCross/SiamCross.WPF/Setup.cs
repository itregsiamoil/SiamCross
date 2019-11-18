﻿using Autofac;
using SiamCross.AppObjects;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.WPF
{
    public class Setup : AppSetup
    {
        protected override void RegisterDependencies(ContainerBuilder cb)
        {
            cb.RegisterType<BluetoothScanerPC>().As<IBluetoothScanner>();

            base.RegisterDependencies(cb);

            cb.RegisterType<BluetoothClassicAdapterPC>().As<IBluetoothClassicAdapter>();
            cb.RegisterType<BluetoothLeAdapterPC>().As<IBluetoothLeAdapter>();
        }
    }
}
