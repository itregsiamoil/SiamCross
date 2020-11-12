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
using SiamCross.Droid.Models.BluetoothAdapters;
using SiamCross.Droid.Services;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Models;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Services.UserOutput;

namespace SiamCross.Droid
{
    public class Setup : AppSetup
    {
        protected override void RegisterDependencies(ContainerBuilder cb)
        {
            cb.RegisterType<BluetoothScannerAndroid>().As<IBluetoothScanner>();

            cb.RegisterType<BluetoothClassicAdapterAndroid>().As<IBluetoothClassicAdapter>();
            cb.RegisterType<BluetoothLeAdapterAndroid>().As<IConnectionBtLe>();
            cb.RegisterType<CustomBluetooth5Adapter>().As<IBluetooth5CustomAdapter>();
            cb.RegisterType<SaveDevicesServiceAndroid>().As<ISaveDevicesService>();
            cb.RegisterType<SQLiteAndroid>().As<ISQLite>();
            cb.RegisterType<FileManagerAndroid>().As<IFileManager>();
            cb.RegisterType<EmailSenderAndroid>().As<IEmailSender>();
            cb.RegisterType<SettingsSaverAndroid>().As<ISettingsSaver>();
            cb.RegisterType<SqliteConnection>().As<IDbConnection>();
            cb.RegisterType<DatabaseCreatorAndroid>().As<IDatabaseCreator>();
            cb.RegisterType<HandbookManagerAndroid>().As<IHandbookManager>();
            cb.RegisterType<NLogManagerAndroid>().As<ILogManager>();
            cb.RegisterType<DefaultAdapterAndroid>().As<IDefaultAdapter>();
            cb.RegisterType<FileOpenDialogAndroid>().As<IFileOpenDialog>();
            //cb.RegisterType<SerialUsbManagerAndroid>().As<ISerialUsbManager>();
            cb.RegisterType<SerialUsbConnector>().As<ISerialUsbManager>();
            base.RegisterDependencies(cb);
        }
    }    
}