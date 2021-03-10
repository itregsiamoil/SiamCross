using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using SiamCross.ViewModels;

namespace SiamCross.Views
{
    public class ViewModelWrap<T> where T : IViewModel
    {
        public T ViewModel { get; }

        public ViewModelWrap()
        {
            using (ILifetimeScope scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>();
            }
        }

        public ViewModelWrap(ScannedDeviceInfo sensorData)
        {
            using (ILifetimeScope scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(ScannedDeviceInfo), sensorData));
            }
        }

        public ViewModelWrap(Ddin2Measurement measurement)
        {
            using (ILifetimeScope scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(Ddin2Measurement), measurement));
            }
        }

        public ViewModelWrap(DuMeasurement measurement)
        {
            using (ILifetimeScope scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(DuMeasurement), measurement));
            }
        }

        public ViewModelWrap(SoundSpeedModel soundSpeedModel)
        {
            using (ILifetimeScope scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(
                        typeof(SoundSpeedModel), soundSpeedModel));
            }
        }
    }
}
