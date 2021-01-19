using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg.Ddin2.Measurement;
using SiamCross.Services;
using SiamCross.Services.Logging;
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
    public class Ddin2MeasurementViewModel : BaseSensorMeasurementViewModel<Ddin2MeasurementStartParameters>, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public string Rod { get; set; }

        private string mStrDynPeriod = Constants.DefaultDynPeriod.ToString("N3", CultureInfo.InvariantCulture);
        private string mStrPumpRate = (60.0f / Constants.DefaultDynPeriod).ToString("N3", CultureInfo.InvariantCulture);
        public float GetPeriodFloatVal(string str)
        {
            float tmp_value = 999999;
            try
            {
                tmp_value = float.Parse(str, CultureInfo.InvariantCulture);
                tmp_value = 60.0f / tmp_value;
            }
            catch (Exception) { }
            if (999999 == tmp_value)
            {
                try
                {
                    tmp_value = float.Parse(str, CultureInfo.CurrentCulture);
                    tmp_value = 60.0f / tmp_value;
                }
                catch (Exception) { }
            }
            return tmp_value;
        }
        public string DynPeriod
        {
            get => mStrDynPeriod;
            set
            {
                if (mStrDynPeriod == value)
                    return;
                float tmp_value = GetPeriodFloatVal(value);
                mStrDynPeriod = value;
                mStrPumpRate = tmp_value.ToString("N3", CultureInfo.InvariantCulture);
                NotifyPropertyChanged("PumpRate");
            }
        }
        public string PumpRate
        {
            get => mStrPumpRate;
            set
            {
                if (mStrPumpRate == value)
                    return;
                float tmp_value = GetPeriodFloatVal(value);
                mStrPumpRate = value;
                mStrDynPeriod = tmp_value.ToString("N3", CultureInfo.InvariantCulture);
                NotifyPropertyChanged("DynPeriod");
            }
        }
        public string ApertNumber { get; set; }
        public string Imtravel { get; set; }
        public ObservableCollection<string> ModelPump { get; set; }
        public string SelectedModelPump { get; set; }
        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        public Ddin2MeasurementViewModel(SensorData sensorData)
            : base(sensorData)
        {
            try
            {
                SensorName = _sensorData.Name;
                ModelPump = new ObservableCollection<string>()
                {
                    Resource.BalancedModelPump,
                    Resource.ChainModelPump,
                    Resource.HydraulicModelPump
                };
                StartMeasurementCommand = new Command(StartMeasurementHandler);

                ValveTestCommand = new Command(() => DependencyService.Get<IToast>().Show(Resource.ValveTest));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ddin2MeasurementVM constructor" + "\n");
                throw;
            }
        }

        protected override void InitMeasurementStartParameters()
        {
            Rod = Constants.DefaultRod.ToString();
            //DynPeriod = Constants.DefaultDynPeriod.ToString();
            ApertNumber = Constants.DefaultApertNumber.ToString();
            Imtravel = Constants.DefaultImtravel.ToString();
            SelectedModelPump = Resource.BalancedModelPump;
            SensorName = _sensorData.Name;

            IEnumerable<Ddin2Measurement> mes
                = DataRepository.Instance.GetDdin2Measurements().
                Where(m => m.Name == SensorName).Select(m => m);

            if (mes.Any())
            {
                Ddin2Measurement _measurement;
                _measurement = mes.Last();
                SelectedField = _measurement.Field;
                Well = _measurement.Well;
                Bush = _measurement.Bush;
                Shop = _measurement.Shop;
                BufferPressure = _measurement.BufferPressure;
                Comments = _measurement.Comment;
                Rod = _measurement.Rod.ToString("N3", CultureInfo.InvariantCulture);
                ApertNumber = _measurement.ApertNumber.ToString();
                Imtravel = _measurement.TravelLength.ToString("N3", CultureInfo.InvariantCulture);
                DynPeriod = (60.0f / _measurement.SwingCount).ToString("N3", CultureInfo.InvariantCulture);
                switch (_measurement.ModelPump)
                {
                    case 0:
                        SelectedModelPump = Resource.BalancedModelPump;
                        break;
                    case 1:
                        SelectedModelPump = Resource.ChainModelPump;
                        break;
                    case 2:
                        SelectedModelPump = Resource.HydraulicModelPump;
                        break;
                    default:
                        break;
                }
            }
        }

        private async void StartMeasurementHandler()
        {
            try
            {
                StartMeasurementCommand = new Command(() => { });
                if (!ValidateForEmptinessEveryParameter())
                {
                    return;
                }

                MeasurementSecondaryParameters secondaryParameters = new MeasurementSecondaryParameters(
                    _sensorData.Name,
                    Resource.Dynamogram,
                    SelectedField,
                    Well,
                    Bush,
                    Shop,
                    BufferPressure,
                    Comments,
                    _sensorData.Battery,
                    _sensorData.Temperature,
                    _sensorData.Firmware,
                    _sensorData.RadioFirmware);

                Ddin2MeasurementStartParameters measurementParams = new Ddin2MeasurementStartParameters(
                    float.Parse(Rod, CultureInfo.InvariantCulture),
                    float.Parse(DynPeriod, CultureInfo.InvariantCulture),
                    int.Parse(ApertNumber),
                    float.Parse(Imtravel, CultureInfo.InvariantCulture),
                    GetModelPump(),
                    secondaryParameters);

                if (!ValidateMeasurementParameters(measurementParams))
                {
                    return;
                }

                await App.Navigation.PopAsync();
                await SensorService.Instance
                    .StartMeasurementOnSensor(_sensorData.Id, measurementParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartMeasurementHandler Ddin2MeauserementVM" + "\n");
                throw;
            }
        }

        private int GetModelPump()
        {
            int result = -1;
            if (SelectedModelPump == Resource.BalancedModelPump)
            {
                result = 0;
            }
            else if (SelectedModelPump == Resource.ChainModelPump)
            {
                result = 1;
            }
            else if (SelectedModelPump == Resource.HydraulicModelPump)
            {
                result = 2;
            }
            return result;
        }

        public string SensorName
        {
            get;
            set;
        }

        protected override bool ValidateMeasurementParameters(Ddin2MeasurementStartParameters measurementParams)
        {
            bool result = true;

            if (!IsNumberInRange(120, 400, measurementParams.Rod))
                _errorList.Add(Resource.RodErrorTextDdin2);
            if (!IsNumberInRange(4000, 300000, measurementParams.DynPeriod))
                _errorList.Add(Resource.DynPeriodErrorTextDdimSiddos);
            if (!IsNumberInRange(1, 5, measurementParams.ApertNumber))
                _errorList.Add(Resource.ApertNumberErrorTextDdimSiddos);
            if (!IsNumberInRange(500, 10000, measurementParams.Imtravel))
                _errorList.Add(Resource.ImtravelErrorTextDdimSiddos);

            if (_errorList.Count != 0)
            {
                ShowErrors();
                result = false;
            }

            return result;
        }

        protected override bool ValidateForEmptinessEveryParameter()
        {
            _errorList.Clear();

            ValidateParameterForEmtpiness(Rod, Resource.RodChoiceText);
            ValidateParameterForEmtpiness(SelectedField, Resource.SelectedFieldChoiceText);
            ValidateParameterForEmtpiness(Well, Resource.WellChoiceText);
            ValidateParameterForEmtpiness(Bush, Resource.BushChoiceText);
            ValidateParameterForEmtpiness(Shop, Resource.ShopChoiceText);
            ValidateParameterForEmtpiness(BufferPressure, Resource.BufferPressureChoiceText);
            ValidateParameterForEmtpiness(Comments, Resource.CommentsChoiceText);
            ValidateParameterForEmtpiness(DynPeriod, Resource.DynPeriodChoiceText);
            ValidateParameterForEmtpiness(PumpRate, Resource.DynPeriodChoiceText);
            ValidateParameterForEmtpiness(ApertNumber, Resource.ApertNumberChoiceText);
            ValidateParameterForEmtpiness(Imtravel, Resource.ImtravelChoiceText);
            ValidateParameterForEmtpiness(SelectedModelPump, Resource.SelectedModelPumpChoiceText);

            if (_errorList.Count != 0)
            {
                ShowErrors();
                return false;
            }

            return true;
        }
    }
}
