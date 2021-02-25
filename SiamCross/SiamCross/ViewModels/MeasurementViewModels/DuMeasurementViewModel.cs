using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Sensors.Du.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Services.Toast;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class DuMeasurementViewModel : BaseSensorMeasurementViewModel<DuMeasurementStartParameters>, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private readonly List<SoundSpeedModel> _soundSpeedModels;
        public ObservableCollection<string> ResearchTypes { get; set; }
        public string SelectedResearchType { get; set; }
        public ObservableCollection<string> SoundSpeedCorrections { get; set; }

        private bool mAmplification = false;
        public bool Amplification
        {
            get => mAmplification;
            set
            {
                mAmplification = value;
                NotifyPropertyChanged(nameof(Amplification));
            }
        }

        private bool mInlet = false;
        public bool Inlet //Впуск
        {
            get => mInlet;
            set
            {
                mInlet = value;
                NotifyPropertyChanged(nameof(Inlet));
            }
        }

        private bool mDepth6000 = false;
        public bool Depth6000 //Впуск
        {
            get => mDepth6000;
            set
            {
                mDepth6000 = value;
                NotifyPropertyChanged(nameof(Depth6000));
            }
        }

        private double _PumpDepth = 0.0;
        public string PumpDepth
        {
            get => _PumpDepth.ToString();
            set
            {
                string group_sep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                string ret = value.Replace(group_sep, string.Empty);
                string cur_sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                string inv_sep = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
                ret = ret.Replace(cur_sep, inv_sep);

                if (double.TryParse(ret, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    if (_PumpDepth != val)
                    {
                        _PumpDepth = val;
                        NotifyPropertyChanged(nameof(PumpDepth));
                    }
                }
            }
        }
        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        private string _selectedSoundSpeedCorrection;
        public string SelectedSoundSpeedCorrection
        {
            get => _selectedSoundSpeedCorrection;
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
            get => _soundSpeed;
            set
            {
                if (!string.IsNullOrEmpty(SelectedSoundSpeedCorrection) && !string.IsNullOrEmpty(value))
                {
                    _selectedSoundSpeedCorrection = null;
                    NotifyPropertyChanged(nameof(SelectedSoundSpeedCorrection));
                }
                _soundSpeed = value;
                NotifyPropertyChanged(nameof(SoundSpeed));
            }
        }

        public string SensorName
        {
            get;
            set;
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
                foreach (SoundSpeedModel elem in _soundSpeedModels)
                {
                    SoundSpeedCorrections.Add(elem.ToString());
                }
                StartMeasurementCommand = new Command(StartMeasurementHandler);
                ValveTestCommand = new Command(() => ToastService.Instance.LongAlert(Resource.ValveTest));

                if (SoundSpeedCorrections.Count != 0)
                {
                    SelectedSoundSpeedCorrection = SoundSpeedCorrections[0];
                }
                SelectedResearchType = ResearchTypes[0];
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DuMeasurementViewModel constructor" + "\n");
                throw;
            }
        }

        private async void StartMeasurementHandler(object obj)
        {
            try
            {
                if (!ValidateForEmptinessEveryParameter()) return;

                StartMeasurementCommand = new Command(() => { });

                if (_selectedSoundSpeedCorrection == null)
                {
                    _selectedSoundSpeedCorrection = "";
                }

                if (_soundSpeed == null)
                {
                    _soundSpeed = "";
                }

                DuMeasurementSecondaryParameters secondaryParameters = new DuMeasurementSecondaryParameters(
                    _sensorData.Name,
                    SelectedResearchType,
                    SelectedField,
                    Well,
                    Bush,
                    Shop,
                    BufferPressure,
                    Comments,
                    _sensorData.Battery,
                    _sensorData.Temperature,
                    _sensorData.Firmware,
                    _sensorData.RadioFirmware,
                    SelectedResearchType,
                    _selectedSoundSpeedCorrection,
                    _soundSpeed);

                DuMeasurementStartParameters measurementParams = new DuMeasurementStartParameters(Amplification,
                    Inlet, Depth6000, secondaryParameters, _PumpDepth);

                await App.Navigation.PopAsync();
                await SensorService.Instance.StartMeasurementOnSensor(_sensorData.Id, measurementParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartMeasurementHandler DuMeasurementVM" + "\n");
                throw;
            }
        }

        protected override void InitMeasurementStartParameters()
        {
            //Чекбоксы "Усиление" и "Впуск" и так инициализируются false
            SoundSpeed = "";
            SelectedSoundSpeedCorrection = "";
            SelectedResearchType = "";
            SensorName = _sensorData.Name;
            _PumpDepth = 0.0;

            IEnumerable<DuMeasurement> mes
                = DataRepository.Instance.GetDuMeasurements().
                Where(m => m.Name == SensorName).Select(m => m);

            if (mes.Any())
            {
                DuMeasurement _measurement;
                _measurement = mes.Last();
                SelectedField = _measurement.Field;
                Well = _measurement.Well;
                Bush = _measurement.Bush;
                Shop = _measurement.Shop;
                BufferPressure = _measurement.BufferPressure;
                _PumpDepth = _measurement.PumpDepth;
                Comments = _measurement.Comment;
                SelectedResearchType = _measurement.MeasurementType;
                SelectedSoundSpeedCorrection = _measurement.SoundSpeedCorrection;
                SoundSpeed = _measurement.SoundSpeed;
            }
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

            if (string.IsNullOrEmpty(SoundSpeed) && string.IsNullOrEmpty(SelectedSoundSpeedCorrection))
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
