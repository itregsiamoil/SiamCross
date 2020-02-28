using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class Ddin2MeasurementViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private SensorData _sensorData;

        private List<string> _errorList;

        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
        public string Comments { get; set; }
        public string Rod { get; set; }
        public string DynPeriod { get; set; }
        public string ApertNumber { get; set; }
        public string Imtravel { get; set; }
        public ObservableCollection<string> ModelPump { get; set; }
        public string SelectedModelPump { get; set; }
        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        public Ddin2MeasurementViewModel(SensorData sensorData)
        {
            try
            {
                _sensorData = sensorData;
                SensorName = _sensorData.Name;
                _errorList = new List<string>();
                Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());
                ModelPump = new ObservableCollection<string>()
                {
                    Resource.BalancedModelPump,
                    Resource.ChainModelPump,
                    Resource.HydraulicModelPump
                };
                StartMeasurementCommand = new Command(StartMeasurementHandler);

                ValveTestCommand = new Command(() => DependencyService.Get<IToast>().Show(Resource.ValveTest));

                InitDefaultValues();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ddin2MeasurementVM constructor");
                throw;
            }
        }

        private void InitDefaultValues()
        {
            Well = Constants.DefaultWell.ToString();
            Bush = Constants.DefaultBush.ToString();
            Shop = Constants.DefaultShop.ToString();
            BufferPressure = Constants.DefaultBufferPressure.ToString();
            //Comments = Constants.DefaultComment;
            Comments = Resource.NoСomment;
            Rod = Constants.DefaultRod.ToString();
            DynPeriod = Constants.DefaultDynPeriod.ToString();
            ApertNumber = Constants.DefaultApertNumber.ToString();
            Imtravel = Constants.DefaultImtravel.ToString();
            //SelectedModelPump = Constants.DefaultModelPump;
            SelectedModelPump = Resource.BalancedModelPump;
        }

        private async void StartMeasurementHandler()
        {
            try
            {
                StartMeasurementCommand = new Command(() => { });
                if (!ValidateForEmptiness())
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
                _logger.Error(ex, "StartMeasurementHandler Ddin2MeauserementVM");
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

        private bool ValidateMeasurementParameters(Ddin2MeasurementStartParameters measurementParams)
        {
            bool result = true;

            if (!IsNumberValid(120, 400, measurementParams.Rod))
                _errorList.Add(Resource.RodErrorTextDdin2);
            if (!IsNumberValid(4000, 300000, measurementParams.DynPeriod))
                _errorList.Add(Resource.DynPeriodErrorTextDdimSiddos);
            if (!IsNumberValid(1, 5, measurementParams.ApertNumber))
                _errorList.Add(Resource.ApertNumberErrorTextDdimSiddos);
            if (!IsNumberValid(500, 10000, measurementParams.Imtravel))
                _errorList.Add(Resource.ImtravelErrorTextDdimSiddos);

            if (_errorList.Count != 0)
            {
                ShowErrors();
                result = false;
            }

            return result;
        }

        private bool IsNumberValid(int from, int to, int number)
        {
            return number >= from && number <= to;
        }

        private bool ValidateForEmptiness()
        {
            _errorList.Clear();

            ValidateParameter(Rod, Resource.RodChoiceText);
            ValidateParameter(SelectedField, Resource.SelectedFieldChoiceText);
            ValidateParameter(Well, Resource.WellChoiceText);
            ValidateParameter(Bush, Resource.BushChoiceText);
            ValidateParameter(Shop, Resource.ShopChoiceText);
            ValidateParameter(BufferPressure, Resource.BufferPressureChoiceText);
            ValidateParameter(Comments, Resource.CommentsChoiceText);
            ValidateParameter(DynPeriod, Resource.DynPeriodChoiceText);
            ValidateParameter(ApertNumber, Resource.ApertNumberChoiceText);
            ValidateParameter(Imtravel, Resource.ImtravelChoiceText);
            ValidateParameter(SelectedModelPump, Resource.SelectedModelPumpChoiceText);

            if (_errorList.Count != 0)
            {
                ShowErrors();
                return false;
            }

            return true;
        }

        private void ShowErrors()
        {
            if (_errorList.Count != 0)
            {
                string errors = "";

                for (int i = 0; i < _errorList.Count; i++)
                {
                    errors += _errorList[i] + Environment.NewLine;
                }

                Application.Current.MainPage.DisplayAlert(Resource.IncorrectDataEnteredErrorText,
                errors, "OK");
            }

        }

        private void ValidateParameter(string text, string errorMessage)
        {
            if (string.IsNullOrEmpty(text) || text == ".")
            {
                _errorList.Add(errorMessage);
            }
        }
    }
}
