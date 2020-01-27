using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Autofac;
using Mono.Data.Sqlite;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.Droid.Models;
using SiamCross.Droid.Services;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Services;

namespace SiamCross.Droid
{
    [Preserve(AllMembers = true)]
    public class Setup : AppSetup
    {
        protected override void RegisterDependencies(ContainerBuilder cb)
        {
            cb.RegisterType<BluetoothScannerAndroid>().As<IBluetoothScanner>();
            base.RegisterDependencies(cb);

            cb.RegisterType<BluetoothClassicAdapterAndroid>().As<IBluetoothClassicAdapter>();
            cb.RegisterType<BluetoothLeAdapterAndroid>().As<IBluetoothLeAdapter>();
            cb.RegisterType<SaveDevicesServiceAndroid>().As<ISaveDevicesService>();
            cb.RegisterType<SQLiteAndroid>().As<ISQLite>();
            cb.RegisterType<FileManagerAndroid>().As<IFileManager>();
            cb.RegisterType<EmailSenderAndroid>().As<IEmailSender>();
            cb.RegisterType<SettingsSaverAndroid>().As<ISettingsSaver>();
            cb.RegisterType<SqliteConnection>().As<IDbConnection>();
            cb.RegisterType<DatabaseCreatorAndroid>().As<IDatabaseCreator>();
            cb.RegisterType<HandbookManagerAndroid>().As<IHandbookManager>();
        }
    }    
}