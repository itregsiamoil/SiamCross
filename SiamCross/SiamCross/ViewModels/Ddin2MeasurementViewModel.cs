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
                "Балансирный",
                "Цепной",
                "Гидравлический"
                };
                StartMeasurementCommand = new Command(StartMeasurementHandler);

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
            Comments = Constants.DefaultComment;
            Rod = Constants.DefaultRod.ToString();
            DynPeriod = Constants.DefaultDynPeriod.ToString();
            ApertNumber = Constants.DefaultApertNumber.ToString();
            Imtravel = Constants.DefaultImtravel.ToString();
            SelectedModelPump = Constants.DefaultModelPump;
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

                if (Imtravel[0] ==
                    Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                {
                    Imtravel.Insert(0, "0");
                }

                var secondaryParameters = new MeasurementSecondaryParameters(
                    _sensorData.Name,
                    "Динамограмма",
                    SelectedField,
                    Well,
                    Bush,
                    Shop,
                    BufferPressure,
                    Comments);

                var measurementParams = new Ddin2MeasurementStartParameters(
                    int.Parse(Rod),
                    int.Parse(DynPeriod),
                    int.Parse(ApertNumber),
                    float.Parse(Imtravel),
                    GetModelPump(),
                    secondaryParameters);

                if (!ValidateMeasurementParameters(measurementParams))
                {
                    ShowErrors();
                    return;
                }

                await Application.Current.MainPage.Navigation.PopModalAsync();
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
            switch (SelectedModelPump)
            {
                case "Балансирный":
                    result = 0;
                    break;
                case "Цепной":
                    result = 1;
                    break;
                case "Гидравлический":
                    result = 2;
                    break;
                default:
                    break;
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

            if (!IsNumberValid(160, 390, measurementParams.Rod))
                _errorList.Add("Диаметр штока должен быть в пределе от 16 до 39!");
            if (!IsNumberValid(4000, 180000, measurementParams.DynPeriod))
                _errorList.Add("Период качания должен быть в пределе от 4 до 180!");
            if (!IsNumberValid(1, 5, measurementParams.ApertNumber))
                _errorList.Add("Номер отверстия должен быть в пределе от 1 до 6!");
            if (!IsNumberValid(500, 9999, measurementParams.Imtravel))
                _errorList.Add("Длина хода должна быть в пределе от 0,5 до 9,999!");

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

            ValidateParameter(SelectedField, "Выберите месторождеине!");
            ValidateParameter(Well, "Выберите скважину!");
            ValidateParameter(Bush, "Введите номер куста!");
            ValidateParameter(Shop, "Введите номер цеха!");
            ValidateParameter(BufferPressure, "Введите буфер давления!");
            ValidateParameter(Comments, "Введите комментарий!");
            ValidateParameter(Rod, "Введите диаметр штока!");
            ValidateParameter(DynPeriod, "Введите период качания!");
            ValidateParameter(ApertNumber, "Введите номер отверствия!");
            ValidateParameter(Imtravel, "Введите длину хода");
            ValidateParameter(SelectedModelPump, "Выберите тип привода!");           

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

                Application.Current.MainPage.DisplayAlert("Введены неправильные данные",
                errors, "OK");
            }

        }

        private void ValidateParameter(string text, string errorMessage)
        {
            if (string.IsNullOrEmpty(text))
            {
                _errorList.Add(errorMessage);
            }
        }
    }
}
