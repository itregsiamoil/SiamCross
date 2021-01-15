using Autofac;
using Mono.Data.Sqlite;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.Droid.Models;
using SiamCross.Droid.Services;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface.Bt2;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Services.UserOutput;
using System.Data;

namespace SiamCross.Droid
{
    public class Setup : AppSetup
    {
        protected override void RegisterDependencies(ContainerBuilder cb)
        {
            cb.RegisterType<DroidBt2Interface>().As<IBt2InterfaceCross>();

            cb.RegisterType<BluetoothScannerAndroid>().As<IBluetoothScanner>();

            cb.RegisterType<BluetoothClassicAdapterAndroid>().As<IConnectionBt2>();
            cb.RegisterType<BluetoothLeAdapterAndroid>().As<IConnectionBtLe>();
            cb.RegisterType<SaveDevicesServiceAndroid>().As<ISaveDevicesService>();
            cb.RegisterType<SQLiteAndroid>().As<ISQLite>();
            cb.RegisterType<FileManagerAndroid>().As<IFileManager>();
            cb.RegisterType<EmailSender>().As<IEmailSender>();
            cb.RegisterType<SettingsSaverAndroid>().As<ISettingsSaver>();
            cb.RegisterType<SqliteConnection>().As<IDbConnection>();
            cb.RegisterType<DatabaseCreatorAndroid>().As<IDatabaseCreator>();
            cb.RegisterType<HandbookManagerAndroid>().As<IHandbookManager>();
            cb.RegisterType<NLogManagerAndroid>().As<ILogManager>();
            cb.RegisterType<DefaultAdapterAndroid>().As<IDefaultAdapter>();
            cb.RegisterType<FileOpenDialogAndroid>().As<IFileOpenDialog>();
            base.RegisterDependencies(cb);
        }
    }    
}