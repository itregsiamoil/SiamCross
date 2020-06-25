using Autofac;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Services;
using SiamCross.ViewModels;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.AppObjects
{
    public class AppSetup
    {
        public IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            RegisterDependencies(containerBuilder);
            return containerBuilder.Build();
        }

        protected virtual void RegisterDependencies(ContainerBuilder cb)
        {
            cb.RegisterType<IBluetoothClassicAdapter>().As<IBluetoothAdapter>();
            cb.RegisterType<IBluetoothLeAdapter>().As<IBluetoothAdapter>();
            cb.RegisterType<IBluetooth5CustomAdapter>().As<IBluetoothAdapter>();

            cb.RegisterType<ScannerViewModel>().SingleInstance();
            cb.RegisterType<ControlPanelPageViewModel>().SingleInstance();
            cb.RegisterType<Ddim2MeasurementViewModel>().AsSelf();
            cb.RegisterType<Ddin2MeasurementViewModel>().AsSelf();
            cb.RegisterType<SiddosA3MMeasurementViewModel>().AsSelf();
            cb.RegisterType<Ddim2MeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<Ddin2MeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<SiddosA3MMeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<DuMeasurementDoneViewModel>().AsSelf();
            cb.RegisterType<DirectoryViewModel>().AsSelf();
            cb.RegisterType<MeasurementsViewModel>().AsSelf();
            cb.RegisterType<MeasurementsSelectionViewModel>().AsSelf();
            cb.RegisterType<SettingsViewModel>().AsSelf(); 
            cb.RegisterType<AddFieldViewModel>().AsSelf();
            cb.RegisterType<DuMeasurementViewModel>().AsSelf();
            cb.RegisterType<SoundSpeedViewModel>().AsSelf();
            cb.RegisterType<SoundSpeedViewViewModel>().AsSelf();
            cb.RegisterType<UmtMeasurementViewModel>().AsSelf();
        }
    }
}
