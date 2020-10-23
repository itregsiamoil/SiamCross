using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.Generic;
using SiamCross.DataBase.DataBaseModels;
using System.Linq;

namespace SiamCross.ViewModels
{
    public class Ddin2MeasurementViewModel : BaseSensorMeasurementViewModel<Ddin2MeasurementStartParameters>, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        public string Rod { get; set; }
        public string PumpRate { get; set; }
        public string ApertNumber { get; set; }
        public string Imtravel { get; set; }
        public ObservableCollection<string> ModelPump { get; set; }
        public string SelectedModelPump { get; set; }
        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        public Ddin2MeasurementViewModel(SensorData sensorData) : base(sensorData)
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
            PumpRate = Constants.DefaultPumpRate.ToString();
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
                PumpRate = _measurement.SwingCount.ToString("N3", CultureInfo.InvariantCulture);
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

                var secondaryParameters = new MeasurementSecondaryParameters(
                    _sensorData.Name,
                    Resource.Dynamogram,
                    SelectedField,
                    Well,
                    Bush,
                    Shop,
                    BufferPressure,
                    Comments);

                var measurementParams = new Ddin2MeasurementStartParameters(
                    float.Parse(Rod, CultureInfo.InvariantCulture),
                    (float)60.0 / float.Parse(PumpRate, CultureInfo.InvariantCulture),
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
