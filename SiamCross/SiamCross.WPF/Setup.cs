﻿using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.WPF.Models;
using SiamCross.WPF.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
            cb.RegisterType<SaveDevicesServicePC>().As<ISaveDevicesService>();
            cb.RegisterType<SQLiteWPF>().As<ISQLite>();
            cb.RegisterType<FileManagerWPF>().As<IFileManager>();
            cb.RegisterType<EmailSenderWPF>().As<IEmailSender>();
            cb.RegisterType<SettingsSaverWPF>().As<ISettingsSaver>();
            cb.RegisterType<SQLiteConnection>().As<IDbConnection>();
            cb.RegisterType<DatabaseCreatorWPF>().As<IDatabaseCreator>();
        }
    }
}
