using Autofac;
using SiamCross.ViewModels;
using SiamCross.ViewModels.Dmg.Survey;
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
            //cb.RegisterType<IBluetoothClassicAdapter>().As<IBluetoothAdapter>();
            //cb.RegisterType<IBluetoothLeAdapter>().As<IBluetoothAdapter>();
            //cb.RegisterType<IBluetooth5CustomAdapter>().As<IBluetoothAdapter>();
            //cb.RegisterType<ScannerViewModel>().SingleInstance();

            cb.RegisterType<ControlPanelPageViewModel>().SingleInstance();
            cb.RegisterType<DynamogrammVM>().AsSelf();
            cb.RegisterType<Ddin2MeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<DuMeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<DuMeasurementViewModel>().AsSelf();

            cb.RegisterType<DirectoryViewModel>().SingleInstance();
            cb.RegisterType<SettingsViewModel>().SingleInstance();
            cb.RegisterType<AddFieldViewModel>().SingleInstance();
            cb.RegisterType<SoundSpeedViewModel>().SingleInstance();
            cb.RegisterType<SoundSpeedViewViewModel>().SingleInstance();
        }
    }
}
