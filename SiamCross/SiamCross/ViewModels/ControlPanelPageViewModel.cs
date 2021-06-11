using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Connection;
using SiamCross.Models.Sensors;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class ControlPanelPageViewModel : BasePageVM
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        public ObservableCollection<ISensor> Sensor { get; }
        public bool IsPreRelease => Version.ToLower().Contains("rc");
        public string Version => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber();

        public ICommand DeleteSensorCommand { get; }
        public ICommand GotoMeasurementPageCommand { get; }
        //public ICommand EnableQickInfoAllCommand => new Command(EnableQickInfoAll);
        //public ICommand DisableQickInfoAllCommand => new Command(DisableQickInfoAll);

        public ControlPanelPageViewModel()
        {
            Sensor = new ObservableCollection<ISensor>();

            SensorService.Instance.SensorAdded += SensorAdded;
            SensorService.Instance.SensorDeleting += SensorDeleted;
            DeleteSensorCommand = new AsyncCommand<Guid>(DeleteSensorHandler
                , (Func<bool>)null, null, false, false);
            GotoMeasurementPageCommand = new AsyncCommand<Guid>(GotoMeasurementPage
                , (Func<bool>)null, null, false, false);

        }
        private async Task DeleteSensorHandler(Guid id)
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
        private async Task GotoMeasurementPage(Guid id)
        {
            try
            {
                ISensor sensor = SensorService.Instance.Sensors
                    .SingleOrDefault(s => s.Id == id);
                if (sensor == null)
                    return;

                switch (sensor.ScannedDeviceInfo.Device.Kind)
                {
                    case 0x1301:
                    case 0x1302:
                    case 0x1303:
                    case 0x1401:
                    case 0x1402:
                    case 0x1403:
                        if (!CanOpenMeasurement(sensor))
                            return;
                        if (!CanOpenPage(typeof(DynamogrammPage)))
                            return;

                        var vm = sensor.SurveysVM.SurveysCollection[0];
                        if (null == vm || !(vm is Dmg.Survey.DynamogrammVM dmgVM))
                            return;
                        dmgVM.InitMeasurementStartParameters();
                        var page = PageNavigator.Get(vm);
                        if (null == page)
                            return;
                        await App.NavigationPage.Navigation.PushAsync(page);

                        break;
                    case 0x1101:
                        if (!CanOpenMeasurement(sensor))
                            return;
                        if (!CanOpenPage(typeof(DuMeasurementPage)))
                            return;
                        await App.NavigationPage.Navigation.PushAsync(
                            new DuMeasurementPage(sensor));
                        break;
                    case 0x1201:
                    case 0x1700:
                        sensor.ShowDetailViewCommand.Execute(this);
                        break;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Message={ex.Message} TRACE={ex.StackTrace}");
                _logger.Error($"Message={ex.Message} TRACE={ex.StackTrace}");
            }
        }
        private bool CanOpenMeasurement(ISensor sensorata)
        {
            bool result = true;
            ISensor sensor = SensorService.Instance.Sensors.SingleOrDefault(
                                s => s.Id == sensorata.Id);
            if (sensor != null)
            {
                if (ConnectionState.Connected == sensor.Connection.State)
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

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            DisableQickInfoAll();
            SensorService.Instance.SensorAdded -= SensorAdded;
            SensorService.Instance.SensorDeleting -= SensorDeleted;

        }
    }//public class ControlPanelPageViewModel : BaseViewModel, IViewModel
}
