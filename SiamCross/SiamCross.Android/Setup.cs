using Android.Runtime;
using Autofac;
using Mono.Data.Sqlite;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.Droid.Models;
using SiamCross.Droid.Models.BluetoothAdapters;
using SiamCross.Droid.Services;
using SiamCross.Droid.Services.MediaScanner;
using SiamCross.Droid.Services.StdDialog;
using SiamCross.Droid.Services.Toast;
using SiamCross.Droid.Utils.FileSystem;
using SiamCross.Models.Adapters.PhyInterface;
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
            //cb.RegisterType<Bt2InterfaceDroid>().As<IBt2InterfaceCross>();
            //cb.RegisterType<BtLeInterfaceDroid>().As<IBtLeInterfaceCross>();
            DependencyService.Register<IBt2InterfaceCross, Bt2InterfaceDroid>();
            DependencyService.Register<IBtLeInterfaceCross, BtLeInterfaceDroid>();

            //cb.RegisterType<ScannerLe>().As<IBluetoothScanner>();
            cb.RegisterType<SettingsSaverAndroid>().As<ISettingsSaver>();
            cb.RegisterType<SqliteConnection>().As<IDbConnection>();
            cb.RegisterType<DatabaseCreatorAndroid>().As<IDatabaseCreator>();
            cb.RegisterType<NLogManagerAndroid>().As<ILogManager>();

            //cb.RegisterType<ConnectionBt2>().As<IConnectionBt2>();
            //cb.RegisterType<ConnectionBtLe>().As<IConnectionBtLe>();
            //DependencyService.Register<IScannerBt2, ScannerBt2>();
            //DependencyService.Register<IScannerLe, ScannerLe >();

            DependencyService.RegisterSingleton<IMediaScanner>(new MediaScanner());
            DependencyService.RegisterSingleton<IEnvironment>(new Environment());
            DependencyService.RegisterSingleton<IFileOpenDialog>(new FileOpenDialog());
            DependencyService.RegisterSingleton<IToast>(new Toast());
            base.RegisterDependencies(cb);
        }
    }
}