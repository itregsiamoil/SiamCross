using Autofac;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
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
            cb.RegisterType<MvxMessengerHub>().As<IMvxMessenger>().SingleInstance();
            cb.RegisterType<ScannedDevicesService>().As<IScannedDevicesService>().SingleInstance();
            cb.RegisterType<IBluetoothClassicAdapter>().As<IBluetoothAdapter>();
            cb.RegisterType<IBluetoothLeAdapter>().As<IBluetoothAdapter>();

            cb.RegisterType<ScannerViewModel>().SingleInstance();
        }
    }
}
