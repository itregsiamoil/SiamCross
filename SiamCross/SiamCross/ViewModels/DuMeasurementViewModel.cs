using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class DuMeasurementViewModel: BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        private List<string> _errorList;
        private SensorData _sensorData;
        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
        public string Comments { get; set; }
        public ObservableCollection<string> ResearchTypes { get; set; }
        public string SelectedReasearchType { get; set; }
        public ObservableCollection<string> SoundSpeedCorrections { get; set; }
        public string SelectedSoundSpeedCorrection { get; set; }
        public bool Amplification { get; set; }
        public bool Inlet { get; set; }
        public string SoundSpeed { get; set; }

        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        public DuMeasurementViewModel(SensorData sensorData)
        {
            try
            {
                _sensorData = sensorData;
                _errorList = new List<string>();
                Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());
                ResearchTypes = new ObservableCollection<string>
                {
                    Resource.DynamicLevel,
                    Resource.StaticLevel
                };
                StartMeasurementCommand = new Command(StartMeasurementHandler);
                ValveTestCommand = new Command(() => DependencyService.Get<IToast>().Show(Resource.ValveTest));
                InitDefaultValues();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ddim2MeasurementViewModel constructor");
                throw;
            }
        }

        private void InitDefaultValues()
        {
            Well = Constants.DefaultWell.ToString();
            Bush = Constants.DefaultBush.ToString();
            Shop = Constants.DefaultShop.ToString();
            BufferPressure = Constants.DefaultBufferPressure.ToString();
            Comments = Resource.NoСomment;            
        }

        private async void StartMeasurementHandler(object obj)
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

                //var measurementParams = new Ddim2MeasurementStartParameters(
                //    float.Parse(DynPeriod, CultureInfo.InvariantCulture),
                //    int.Parse(ApertNumber),
                //    float.Parse(Imtravel, CultureInfo.InvariantCulture),
                //    GetModelPump(),
                //    secondaryParameters);

                if (!ValidateMeasurementParameters(measurementParams))
                {
                    return;
                }

                await App.Navigation.PopAsync();
                await SensorService.Instance.StartMeasurementOnSensor(_sensorData.Id, measurementParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartMeasurementHandler Ddim2MeasurementVM");
                throw;
            }
        }

        private bool ValidateMeasurementParameters(DuMeasurementStartParameters measurementParams)
        {
            bool result = true;

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

            ValidateParameter(SelectedField, Resource.SelectedFieldChoiceText);
            ValidateParameter(Well, Resource.WellChoiceText);
            ValidateParameter(Bush, Resource.BushChoiceText);
            ValidateParameter(Shop, Resource.ShopChoiceText);
            ValidateParameter(BufferPressure, Resource.BufferPressureChoiceText);
            ValidateParameter(Comments, Resource.CommentsChoiceText);


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
