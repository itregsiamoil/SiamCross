﻿using Autofac;
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
using System.Windows.Input;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public abstract class BaseSensorMeasurementViewModel<T> : INotifyPropertyChanged, IHaveSecondaryParameters
        where T:class
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        protected SensorData _sensorData;
        protected List<string> _errorList;

        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField { get; set; }
        public string Well { get; set; }
        public string Bush { get; set; }
        public string Shop { get; set; }
        public string BufferPressure { get; set; }
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
                var fieldList = HandbookData.Instance.GetFieldList();
                foreach (var field in fieldList)
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
                var stack = App.NavigationPage.Navigation.ModalStack;
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

        protected bool IsNumberInRange(int from, int to, int number)
        {
            return number >= from && number <= to;
        }

        protected void InitParametersWithDefaultValues()
        {
            Well = Constants.DefaultWell.ToString();
            Bush = Constants.DefaultBush.ToString();
            Shop = Constants.DefaultShop.ToString();
            BufferPressure = Constants.DefaultBufferPressure.ToString();
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
