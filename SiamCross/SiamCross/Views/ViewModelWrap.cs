using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Tools;
using SiamCross.ViewModels;
using System.Collections.ObjectModel;

namespace SiamCross.Views
{
    public class ViewModelWrap<T> where T : IViewModel
    {
        public T ViewModel { get; }

        public ViewModelWrap()
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>();
            }
        }

        public ViewModelWrap(SensorData sensorData)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(SensorData), sensorData));
            }
        }

        public ViewModelWrap(Ddim2Measurement measurement)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(Ddim2Measurement), measurement));
            }
        }

        public ViewModelWrap(Ddin2Measurement measurement)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(Ddin2Measurement), measurement));
            }
        }

        public ViewModelWrap(SiddosA3MMeasurement measurement)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(SiddosA3MMeasurement), measurement));
            }
        }

        public ViewModelWrap(DuMeasurement measurement)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(DuMeasurement), measurement));
            }
        }

        public ViewModelWrap(ObservableCollection<MeasurementView> measurements)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(
                        typeof(ObservableCollection<MeasurementView>), measurements));
            }
        }

        public ViewModelWrap(SoundSpeedModel soundSpeedModel)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                ViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(
                        typeof(SoundSpeedModel), soundSpeedModel));
            }
        }
    }
}
