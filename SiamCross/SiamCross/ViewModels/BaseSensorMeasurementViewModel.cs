using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public abstract class BaseSensorMeasurementViewModel<T> : INotifyPropertyChanged
        where T : class
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        protected SensorData _sensorData;
        protected List<string> _errorList;

        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }

        protected double _BufferPressure = 0.0;
        public string BufferPressure
        {
            get => _BufferPressure.ToString();
            set
            {
                string group_sep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                string ret = value.Replace(group_sep, string.Empty);
                string cur_sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                string inv_sep = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
                ret = ret.Replace(cur_sep, inv_sep);

                if (double.TryParse(ret, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    if (_BufferPressure != val)
                    {
                        _BufferPressure = val;
                        NotifyPropertyChanged(nameof(BufferPressure));
                    }
                }
            }
        }



        public string Comments { get; set; }

        public ICommand AddField { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseSensorMeasurementViewModel(SensorData sensorData)
        {
            _sensorData = sensorData;
            _errorList = new List<string>();
            Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());
            AddField = new Command(AddNewField);
            InitParametersWithDefaultValues();
            MessagingCenter
                .Subscribe<AddFieldViewModel>(
                    this,
                    "Refresh",
                    (sender) =>
                    {
                        try
                        {
                            UpdateFields();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Refresh fields BaseSensorMeasurementViewModel" + "\n");
                        }
                    }
                );
        }

        private void UpdateFields()
        {
            try
            {
                Fields.Clear();
                IEnumerable<string> fieldList = HandbookData.Instance.GetFieldList();
                foreach (string field in fieldList)
                {
                    Fields.Add(field);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update method" + "\n");
                throw;
            }
        }

        protected void AddNewField()
        {
            try
            {
                IReadOnlyList<Page> stack = App.NavigationPage.Navigation.ModalStack;
                if (stack.Count > 0)
                {
                    if (stack[stack.Count - 1].GetType() != typeof(AddFieldPage))
                    {
                        App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                    }
                }
                else
                {
                    App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "OpenAddFieldsPage command handler" + "\n");
                throw;
            }
        }

        protected static bool IsNumberInRange<TDATA>(TDATA from, TDATA to, TDATA number)
            where TDATA : System.IComparable<TDATA>
        {
            return 0 <= number.CompareTo(from) && 0 >= number.CompareTo(to);
        }

        protected void InitParametersWithDefaultValues()
        {
            Well = Constants.DefaultWell.ToString();
            Bush = Constants.DefaultBush.ToString();
            Shop = Constants.DefaultShop.ToString();
            _BufferPressure = Constants.DefaultBufferPressure;
            Comments = Resource.NoСomment;

            InitMeasurementStartParameters();
        }

        protected abstract void InitMeasurementStartParameters();
        protected abstract bool ValidateForEmptinessEveryParameter();
        protected abstract bool ValidateMeasurementParameters(T measurementParameters);

        protected void ValidateParameterForEmtpiness(string text, string errorMessage)
        {
            if (string.IsNullOrEmpty(text) || text == ".")
            {
                _errorList.Add(errorMessage);
            }
        }

        protected void ShowErrors()
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

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
