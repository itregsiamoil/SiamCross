using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Sensors;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.ViewModels.MeasurementViewModels;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    public abstract class BaseSensorMeasurementViewModel<T> : BaseSurveyVM
        where T : class
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        protected List<string> _errorList;

        private readonly ObservableCollection<string> _Fields = new ObservableCollection<string>();
        public ObservableCollection<string> Fields => _Fields;
        public string SelectedField { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }

        protected string _BufferPressure = "0.0";
        public string BufferPressure
        {
            get => _BufferPressure;
            set => _BufferPressure = value;
        }
        public string Comments { get; set; }
        public ICommand AddField { get; set; }


        public BaseSensorMeasurementViewModel(ISensor sensor, BaseSurvey model)
            : base(sensor, model)
        {
            _errorList = new List<string>();
            AddField = new Command(AddNewFieldAsync);
            InitParametersWithDefaultValues();
            Repo.FieldDir.FieldList.ForEach(o => _Fields.Add(o.Title));
            Repo.FieldDir.FieldList.CollectionChanged += FieldList_CollectionChanged;
        }
        private void FieldList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _Fields.Clear();
            Repo.FieldDir.FieldList.ForEach(o => _Fields.Add(o.Title));
            ChangeNotify(nameof(SelectedField));
        }
        protected async void AddNewFieldAsync()
        {
            try
            {
                IReadOnlyList<Page> stack = App.NavigationPage.Navigation.ModalStack;
                if (stack.Count > 0)
                {
                    if (stack[stack.Count - 1].GetType() != typeof(AddFieldPage))
                    {
                        await App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
                    }
                }
                else
                {
                    await App.NavigationPage.Navigation.PushModalAsync(new AddFieldPage());
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
            _BufferPressure = Constants.DefaultBufferPressure.ToString();
            Comments = Resource.NoСomment;

            InitMeasurementStartParameters();
        }

        public abstract void InitMeasurementStartParameters();
        protected abstract bool ValidateForEmptinessEveryParameter();
        protected abstract bool ValidateMeasurementParameters(T measurementParameters);

        protected bool TryToDouble(string text, out double val)
        {
            string group_sep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            string ret = text.Replace(group_sep, string.Empty);
            string cur_sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string inv_sep = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
            ret = ret.Replace(cur_sep, inv_sep);
            return double.TryParse(ret, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
        }


        protected void ValidateParameterForDouble(string text, string errorMessage)
        {
            if (!TryToDouble(text, out _))
            {
                _errorList.Add(errorMessage);
            }
        }
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

    }
}
