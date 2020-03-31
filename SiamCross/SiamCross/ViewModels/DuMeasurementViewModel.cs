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
using System.Collections.Generic;

namespace SiamCross.ViewModels
{
    public class DuMeasurementViewModel: BaseSensorMeasurementViewModel<DuMeasurementStartParameters>, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private List<SoundSpeedModel> _soundSpeedModels;
        public ObservableCollection<string> ResearchTypes { get; set; }
        public string SelectedResearchType { get; set; }
        public ObservableCollection<string> SoundSpeedCorrections { get; set; }
        public bool Amplification { get; set; }
        public bool Inlet { get; set; } //Впуск
        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        private string _selectedSoundSpeedCorrection;
        public string SelectedSoundSpeedCorrection
        {
            get
            {
                return _selectedSoundSpeedCorrection;
            }
            set
            {
                if (!string.IsNullOrEmpty(_soundSpeed))
                {
                    _soundSpeed = null;
                    NotifyPropertyChanged(nameof(SoundSpeed));                 
                }
                _selectedSoundSpeedCorrection = value;
                NotifyPropertyChanged(nameof(SelectedSoundSpeedCorrection));
            }
        }
        private string _soundSpeed;
        public string SoundSpeed
        {
            get
            {
                return _soundSpeed;
            }
            set
            {
                if (!string.IsNullOrEmpty(SelectedSoundSpeedCorrection))
                {
                    _selectedSoundSpeedCorrection = null;
                    NotifyPropertyChanged(nameof(SelectedSoundSpeedCorrection));
                }
                _soundSpeed = value;
                NotifyPropertyChanged(nameof(SoundSpeed));
            }
        }

        public DuMeasurementViewModel(SensorData sensorData) : base(sensorData)
        {
            try
            {
                _soundSpeedModels = HandbookData.Instance.GetSoundSpeedList();
                ResearchTypes = new ObservableCollection<string>
                {
                    Resource.DynamicLevel,
                    Resource.StaticLevel
                };
                SoundSpeedCorrections = new ObservableCollection<string>();
                foreach (var elem in _soundSpeedModels)
                {
                    SoundSpeedCorrections.Add(elem.ToString());
                }
                StartMeasurementCommand = new Command(StartMeasurementHandler);
                ValveTestCommand = new Command(() => DependencyService.Get<IToast>().Show(Resource.ValveTest));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DuMeasurementViewModel constructor");
                throw;
            }
        }

        private async void StartMeasurementHandler(object obj)
        {
            try
            {
                if (!ValidateForEmptinessEveryParameter()) return;

                StartMeasurementCommand = new Command(() => { });

                var secondaryParameters = new DuMeasurementSecondaryParameters(
                    _sensorData.Name,
                    SelectedResearchType,
                    SelectedField,
                    Well,
                    Bush,
                    Shop,
                    BufferPressure,
                    Comments,
                    SelectedResearchType,
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
                _logger.Error(ex, "StartMeasurementHandler DuMeasurementVM");
                throw;
            }
        }

        protected override void InitMeasurementStartParameters()
        {
            //Чекбоксы "Усиление" и "Впуск" и так инициализируются false
            SoundSpeed = "";
            SelectedSoundSpeedCorrection = "";
            SelectedResearchType = "";
        }

        protected override bool ValidateForEmptinessEveryParameter()
        {
            base._errorList.Clear();

            base.ValidateParameterForEmtpiness(SelectedField, Resource.SelectedFieldChoiceText);
            base.ValidateParameterForEmtpiness(Well, Resource.WellChoiceText);
            base.ValidateParameterForEmtpiness(Bush, Resource.BushChoiceText);
            base.ValidateParameterForEmtpiness(Shop, Resource.ShopChoiceText);
            base.ValidateParameterForEmtpiness(BufferPressure, Resource.BufferPressureChoiceText);
            base.ValidateParameterForEmtpiness(Comments, Resource.CommentsChoiceText);
            base.ValidateParameterForEmtpiness(SelectedResearchType, Resource.SelectedReasearchTypeChoice);
            
            if(string.IsNullOrEmpty(SoundSpeed) && string.IsNullOrEmpty(SelectedSoundSpeedCorrection))
            {
                _errorList.Add(Resource.ChoiceSpeedCorrectionTableOrInpunSpeed);
            }

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
