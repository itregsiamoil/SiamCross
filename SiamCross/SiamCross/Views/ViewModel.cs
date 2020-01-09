using Autofac;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.ViewModels;
using System.Collections.ObjectModel;

namespace SiamCross.Views
{
    public class ViewModel<T> where T:IViewModel
    {
        public T GetViewModel { get; }

        public ViewModel()
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                GetViewModel = AppContainer.Container.Resolve<T>();
            }
        }

        public ViewModel(SensorData sensorData)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                GetViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(SensorData), sensorData));
            }
        }

        public ViewModel(Ddim2Measurement measurement)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                GetViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(Ddim2Measurement), measurement));
            }
        }

        public ViewModel(Ddin2Measurement measurement)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                GetViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(typeof(Ddin2Measurement), measurement));
            }
        }

        public ViewModel(ObservableCollection<MeasurementView> measurements)
        {
            using (var scope = AppContainer.Container.BeginLifetimeScope())
            {
                GetViewModel = AppContainer.Container.Resolve<T>(
                    new TypedParameter(
                        typeof(ObservableCollection<MeasurementView>), measurements));
            }
        }
    }
}
