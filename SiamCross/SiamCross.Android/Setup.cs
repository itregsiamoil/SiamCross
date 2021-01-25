using Android.Runtime;
using Autofac;
using Mono.Data.Sqlite;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.Droid.Models;
using SiamCross.Droid.Services;
using SiamCross.Droid.Services.MediaScanner;
using SiamCross.Droid.Services.StdDialog;
using SiamCross.Droid.Services.Toast;
using SiamCross.Droid.Utils.FileSystem;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface.Bt2;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.Services.Environment;
using SiamCross.Services.Logging;
using SiamCross.Services.MediaScanner;
using SiamCross.Services.StdDialog;
using SiamCross.Services.Toast;
using System.Data;
using Xamarin.Forms;

namespace SiamCross.Droid
{
    [Preserve(AllMembers = true)]
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
            cb.RegisterType<SettingsSaverAndroid>().As<ISettingsSaver>();
            cb.RegisterType<SqliteConnection>().As<IDbConnection>();
            cb.RegisterType<DatabaseCreatorAndroid>().As<IDatabaseCreator>();
            cb.RegisterType<HandbookManagerAndroid>().As<IHandbookManager>();
            cb.RegisterType<NLogManagerAndroid>().As<ILogManager>();
            cb.RegisterType<DefaultAdapterAndroid>().As<IDefaultAdapter>();

            DependencyService.RegisterSingleton<IMediaScanner>(new MediaScanner());
            DependencyService.RegisterSingleton<IEnvironment>(new Environment());
            DependencyService.RegisterSingleton<IFileOpenDialog>(new FileOpenDialog());
            DependencyService.RegisterSingleton<IToast>(new Toast());
            base.RegisterDependencies(cb);
        }
    }
}