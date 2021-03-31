using Autofac;
using SiamCross.Services;
using SiamCross.ViewModels;
using SiamCross.ViewModels.Dmg;
using SiamCross.ViewModels.Dmg.Survey;
using SiamCross.ViewModels.Dua;
using SiamCross.Views;
using SiamCross.Views.DDIN2;
using SiamCross.Views.Dua;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.AppObjects
{
    [Preserve(AllMembers = true)]
    public class AppSetup
    {
        public IContainer CreateContainer()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            RegisterDependencies(containerBuilder);
            return containerBuilder.Build();
        }

        protected virtual void RegisterDependencies(ContainerBuilder cb)
        {
            ViewFactoryService.Register(typeof(PositionInfoVM), () => new PositionEditPage());
            ViewFactoryService.Register(typeof(SensorDetailsVM), () => new SensorDetailsPage());

            ViewFactoryService.Register(typeof(DmgDownloadViewModel), () => new DmgDownloadPage());
            ViewFactoryService.Register(typeof(ViewModels.Dmg.Survey.DynamogrammVM)
                , () => new DynamogrammPage());



            ViewFactoryService.Register(typeof(DuaDownloadViewModel), () => new DuaDownloadPage());
            ViewFactoryService.Register(typeof(FactoryConfigVM), () => new FactoryConfigPage());
            ViewFactoryService.Register(typeof(UserConfigVM), () => new UserConfigPage());
            //ViewFactoryService.Register(typeof(StateVM), () => new StatePage());
            ViewFactoryService.Register(typeof(SurveysCollectionVM), () => new SurveysCollectionPage());

            ViewFactoryService.Register(typeof(ViewModels.Dua.Survey.StaticLevelVM)
                , () => new StaticLevelPage());





            //cb.RegisterType<IBluetoothClassicAdapter>().As<IBluetoothAdapter>();
            //cb.RegisterType<IBluetoothLeAdapter>().As<IBluetoothAdapter>();
            //cb.RegisterType<IBluetooth5CustomAdapter>().As<IBluetoothAdapter>();
            //cb.RegisterType<ScannerViewModel>().SingleInstance();

            cb.RegisterType<ControlPanelPageViewModel>().SingleInstance();
            cb.RegisterType<DynamogrammVM>().AsSelf();
            cb.RegisterType<Ddin2MeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<DuMeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<DirectoryViewModel>().AsSelf();
            cb.RegisterType<SettingsViewModel>().AsSelf();
            cb.RegisterType<AddFieldViewModel>().AsSelf();
            cb.RegisterType<DuMeasurementViewModel>().AsSelf();
            cb.RegisterType<SoundSpeedViewModel>().AsSelf();
            cb.RegisterType<SoundSpeedViewViewModel>().AsSelf();
        }

        public async Task Init()
        {
            await DataRepository.Instance.Init();
            await HandbookData.Instance.Init();
        }
    }
}
