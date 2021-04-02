using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Sensors;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Views;
using SiamCross.Views.MenuItems;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class ControlPanelPageViewModel : BaseVM
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        public ObservableCollection<ISensor> Sensor { get; }
        public bool IsRelease => !Version.ToLower().Contains("rc");
        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();
        public ControlPanelPageViewModel()
        {
            Sensor = new ObservableCollection<ISensor>();

            SensorService.Instance.SensorAdded += SensorAdded;
            SensorService.Instance.SensorDeleting += SensorDeleted;
        }
        public ICommand RecentMeasurementCommand => new Command<Guid>(RecentMeasurement);
        public ICommand DeleteSensorCommand => new Command<Guid>(DeleteSensorHandler);
        public ICommand GotoMeasurementPageCommand => new Command<Guid>(GotoMeasurementPage);
        //public ICommand EnableQickInfoAllCommand => new Command(EnableQickInfoAll);
        //public ICommand DisableQickInfoAllCommand => new Command(DisableQickInfoAll);
        private void RecentMeasurement(Guid id)
        {
            try
            {
                ISensor sensor = SensorService.Instance.Sensors
                    .SingleOrDefault(s => s.Id == id);
                if (sensor != null)
                {
                    if (CanOpenPage(typeof(MeasurementsPage)))
                    {
                        App.NavigationPage.Navigation.PushAsync(new MeasurementsPage());
                        App.MenuIsPresented = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteSensorHandler" + "\n");
                throw;
            }
        }
        private async void DeleteSensorHandler(Guid id)
        {
            try
            {
                ISensor sensor = SensorService.Instance.Sensors
                    .FirstOrDefault(s => s.Id == id);
                if (sensor != null)
                {
                    if (!sensor.IsMeasurement)
                    {
                        await SensorService.Instance.DeleteSensorAsync(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteSensorHandler" + "\n");

                //throw;
            }
        }
        private void SensorDeleted(ISensor sensor)
        {
            try
            {
                Sensor.Remove(sensor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteSensorHandler" + "\n");
                //throw;
            }
        }
        private void SensorAdded(ISensor sensor)
        {

            Debug.WriteLine("SensorAdded in view");
            if (!Sensor.Contains(sensor))
            {
                Sensor.Add(sensor);
            }
        }
        private void GotoMeasurementPage(Guid id)
        {
            ISensor sensor = SensorService.Instance.Sensors
                .SingleOrDefault(s => s.Id == id);
            if (sensor == null)
                return;
            if (!CanOpenMeasurement(sensor))
                return;

            switch (sensor.ScannedDeviceInfo.Device.Kind)
            {
                case 0x1301:
                case 0x1302:
                case 0x1303:
                case 0x1401:
                case 0x1402:
                case 0x1403:
                    if (!CanOpenPage(typeof(DynamogrammPage)))
                        return;

                    var vm = sensor.Surveys[0];
                    if (null == vm)
                        return;
                    var page = ViewFactoryService.Get(vm);
                    if (null == page)
                        return;
                    App.NavigationPage.Navigation.PushAsync(page);
                    break;
                case 0x1101:
                    if (!CanOpenPage(typeof(DuMeasurementPage)))
                        return;
                    App.NavigationPage.Navigation.PushAsync(
                        new DuMeasurementPage(sensor));
                    break;
                case 0x1201:
                    sensor.ShowDetailViewCommand.Execute(this);
                    break;
                default: break;
            }

        }
        private bool CanOpenMeasurement(ISensor sensorata)
        {
            bool result = true;
            ISensor sensor = SensorService.Instance.Sensors.SingleOrDefault(
                                s => s.Id == sensorata.Id);
            if (sensor != null)
            {
                if (sensor.IsAlive)
                {
                    if (sensor.IsMeasurement)
                        result = false;
                }
                else result = false;
            }

            return result;
        }
        public void EnableQickInfoAll()
        {
            foreach (var sensor in Sensor)
                sensor.IsEnableQickInfo = true;
        }
        public void DisableQickInfoAll()
        {
            foreach (var sensor in Sensor)
                sensor.IsEnableQickInfo = false;
        }
        private bool CanOpenModalPage(Type type)
        {
            bool result = false;
            System.Collections.Generic.IReadOnlyList<Page> stack = App.NavigationPage.Navigation.ModalStack;

            if (stack.Count > 0)
            {
                if (stack[stack.Count - 1].GetType() != type)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }
        private bool CanOpenPage(Type type)
        {
            try
            {
                System.Collections.Generic.IReadOnlyList<Page> stack = App.NavigationPage.Navigation.NavigationStack;
                if (stack[stack.Count - 1].GetType() != type)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CanOpenPage method" + "\n");
                throw;
            }
        }
    }//public class ControlPanelPageViewModel : BaseViewModel, IViewModel
}
