using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class DuMeasurementViewModel: BaseSensorMeasurementViewModel<DuMeasurementStartParameters>, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        
        public ObservableCollection<string> ResearchTypes { get; set; }
        public string SelectedReasearchType { get; set; }
        public ObservableCollection<string> SoundSpeedCorrections { get; set; }
        public string SelectedSoundSpeedCorrection { get; set; }
        public bool Amplification { get; set; }
        public bool Inlet { get; set; }
        public string SoundSpeed { get; set; }

        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        public DuMeasurementViewModel(SensorData sensorData) : base(sensorData)
        {
            try
            {
                ResearchTypes = new ObservableCollection<string>
                {
                    Resource.DynamicLevel,
                    Resource.StaticLevel
                };
                SoundSpeedCorrections = new ObservableCollection<string>();
                foreach (var elem in HandbookData.Instance.GetSoundSpeedList())
                {
                    SoundSpeedCorrections.Add(elem.ToString());
                }
                StartMeasurementCommand = new Command(StartMeasurementHandler);
                ValveTestCommand = new Command(() => DependencyService.Get<IToast>().Show(Resource.ValveTest));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ddim2MeasurementViewModel constructor");
                throw;
            }
        }

        private async void StartMeasurementHandler(object obj)
        {
            try
            {
                StartMeasurementCommand = new Command(() => { });
                //if (!ValidateForEmptinessEveryParameter())
                //{
                //    return;
                //}

                var secondaryParameters = new DuMeasurementSecondaryParameters(
                    _sensorData.Name,
                    Resource.Dynamogram,
                    SelectedField,
                    Well,
                    Bush,
                    Shop,
                    BufferPressure,
                    Comments,
                    SelectedReasearchType,
                    SelectedSoundSpeedCorrection.ToString(),
                    SoundSpeed);

                var measurementParams = new DuMeasurementStartParameters(Amplification,
                    Inlet,
                    secondaryParameters);

                await App.Navigation.PopAsync();
                await SensorService.Instance.StartMeasurementOnSensor(_sensorData.Id, measurementParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartMeasurementHandler Ddim2MeasurementVM");
                throw;
            }
        }

        protected override void InitMeasurementStartParameters()
        {
            //Чекбоксы "Усиление" и "Впуск" и так инициализируются false
            SoundSpeed = "";
            SelectedSoundSpeedCorrection = "";
            SelectedReasearchType = "";
        }

        protected override bool ValidateForEmptinessEveryParameter()
        {
            _errorList.Clear();

            ValidateParameterForEmtpiness(SelectedField, Resource.SelectedFieldChoiceText);
            ValidateParameterForEmtpiness(Well, Resource.WellChoiceText);
            ValidateParameterForEmtpiness(Bush, Resource.BushChoiceText);
            ValidateParameterForEmtpiness(Shop, Resource.ShopChoiceText);
            ValidateParameterForEmtpiness(BufferPressure, Resource.BufferPressureChoiceText);
            ValidateParameterForEmtpiness(Comments, Resource.CommentsChoiceText);
            ValidateParameterForEmtpiness(SelectedReasearchType, Resource.SelectedReasearchTypeChoice);
            ValidateParameterForEmtpiness(SelectedSoundSpeedCorrection, Resource.SelectedSoundSpeedCorrectionChoice);

            if (_errorList.Count != 0)
            {
                ShowErrors();
                return false;
            }

            return true;
        }

        protected override bool ValidateMeasurementParameters(DuMeasurementStartParameters measurementParameters)
        {
            return true;
        }
    }
}
