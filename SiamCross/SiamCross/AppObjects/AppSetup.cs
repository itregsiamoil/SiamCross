using Autofac;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Services;
using SiamCross.ViewModels;

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

            cb.RegisterType<ScannedDevicesService>().As<IScannedDevicesService>().SingleInstance();
            cb.RegisterType<IBluetoothClassicAdapter>().As<IBluetoothAdapter>();
            cb.RegisterType<IBluetoothLeAdapter>().As<IBluetoothAdapter>();

            cb.RegisterType<ScannerViewModel>().SingleInstance();
        }
    }
}
