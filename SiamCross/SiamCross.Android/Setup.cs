using Android.Runtime;
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
        public override void RegisterDependencies()
        {
            DependencyService.RegisterSingleton<ILogManager>(new NLogManagerAndroid());


            DependencyService.Register<IBt2InterfaceCross, Bt2InterfaceDroid>();
            DependencyService.Register<IBtLeInterfaceCross, BtLeInterfaceDroid>();

            DependencyService.Register<IDbConnection, SqliteConnection>();
            DependencyService.Register<IDatabaseCreator, DatabaseCreatorAndroid>();
            DependencyService.Register<ILogManager, NLogManagerAndroid>();

            //cb.RegisterType<SqliteConnection>().As<IDbConnection>();
            //cb.RegisterType<DatabaseCreatorAndroid>().As<IDatabaseCreator>();
            //cb.RegisterType<NLogManagerAndroid>().As<ILogManager>();

            DependencyService.RegisterSingleton<IMediaScanner>(new MediaScanner());
            DependencyService.RegisterSingleton<IEnvironment>(new Environment());
            DependencyService.RegisterSingleton<IFileOpenDialog>(new FileOpenDialog());
            DependencyService.RegisterSingleton<IToast>(new Toast());
            base.RegisterDependencies();
        }
    }
}