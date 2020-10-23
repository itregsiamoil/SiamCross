using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Umt.Measurement;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels.MeasurementViewModels
{
    public class UmtMeasurementViewModel : BaseSensorMeasurementViewModel<UmtMeasurementStartParameters>,
        IViewModel
    {

        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public bool IsTemperatureMesure { get; set; }

        public string SensorName
        {
            get;
            set;
        }

        public string Interval { get; set; }

        public string SelectedMeasurementType { get; set; }

        public ObservableCollection<string> MeasurementTypes { get; set; }

        protected override void InitMeasurementStartParameters()
        {
            Interval = Constants.DefaultUmtMeasurementInterval.ToString();
            IsTemperatureMesure = false;
            SelectedMeasurementType = Resource.StaticPressureSingle;
        }

        protected override bool ValidateForEmptinessEveryParameter()
        {
            _errorList.Clear();

            ValidateParameterForEmtpiness(SelectedField, Resource.SelectedFieldChoiceText);
            ValidateParameterForEmtpiness(Well, Resource.WellChoiceText);
            ValidateParameterForEmtpiness(Bush, Resource.BushChoiceText);
            ValidateParameterForEmtpiness(Shop, Resource.ShopChoiceText);

            ValidateParameterForEmtpiness(Interval, Resource.IntervalChoiceText);
            ValidateParameterForEmtpiness(SelectedMeasurementType, Resource.SelectedMeasurementTypeChoiceText);

            if (_errorList.Count != 0)
            {
                ShowErrors();
                return false;
            }

            return true;
        }

        protected override bool ValidateMeasurementParameters(UmtMeasurementStartParameters measurementParameters)
        {
            if (!IsNumberInRange(1, 82800, measurementParameters.Interval))
            {
                _errorList.Add(Resource.IntervalErrorTextUmt);
            }

            if (_errorList.Count != 0)
            {
                ShowErrors();
                return false;
            }

            return true;
        }

        public UmtMeasurementViewModel(SensorData sensorData) : base(sensorData)
        {
            try
            {
                SensorName = _sensorData.Name;
                MeasurementTypes = new ObservableCollection<string>
                {
                    Resource.StaticPressureSingle,
                    Resource.DynamicPressureSingle,
                    Resource.StaticPressure,
                    Resource.DynamicPressure
                };

                StartMeasurementCommand = new Command(StartMeasurementHandler);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UmtMeasurementViewModel constructor" + "\n");
                throw;
            }
        }

        public ICommand ZeroingCommand { get; set; }
        public ICommand DownloadCommand { get; set; }
        public ICommand CancelMeasureCommand { get; set; }
        public ICommand ClearMemmoryCommand { get; set; }
        public ICommand StartMeasurementCommand { get; set; }

        private async void StartMeasurementHandler()
        {
            try
            {
                StartMeasurementCommand = new Command(() => { });
                if(!ValidateForEmptinessEveryParameter())
                {
                    return;
                }

                var secondaryParameters = new MeasurementSecondaryParameters(
                    _sensorData.Name,
                    Resource.Dynamogram,
                    SelectedField,
                    Well,
                    Bush,
                    Shop,
                    BufferPressure,
                    Comments);

                var measurementParams = new UmtMeasurementStartParameters(
                    GetMeasurementType(),
                    int.Parse(Interval),
                    IsTemperatureMesure,
                    secondaryParameters
                    );
                if (!ValidateMeasurementParameters(measurementParams))
                {
                    return;
                }

                await App.Navigation.PopAsync();
                await SensorService.Instance.StartMeasurementOnSensor(_sensorData.Id, measurementParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartMeasurementHandler UmtMeasurementVM" + "\n");
                throw;
            }
        }
        

        private UmtMeasurementType GetMeasurementType()
        {
            var result = UmtMeasurementType.StaticPressureSingle;
            if (SelectedMeasurementType == Resource.StaticPressureSingle)
            {
                result = UmtMeasurementType.StaticPressureSingle;
            }
            else if (SelectedMeasurementType == Resource.StaticPressure)
            {
                result = UmtMeasurementType.StaticPressure;
            }
            else if (SelectedMeasurementType == Resource.DynamicPressureSingle)
            {
                result = UmtMeasurementType.DynamicPressureSingle;
            }
            else if (SelectedMeasurementType == Resource.DynamicPressure)
            {
                result = UmtMeasurementType.DynamicPressure;

            }
            return result;
        }
    }
}
