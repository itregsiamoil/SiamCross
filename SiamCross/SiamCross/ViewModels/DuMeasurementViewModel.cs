﻿using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Du.Measurement;
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
    public class DuMeasurementViewModel: BaseSensorMeasurementViewModel<DuMeasurementStartParameters>, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();
        
        public ObservableCollection<string> ResearchTypes { get; set; }
        public string SelectedReasearchType { get; set; }
        public ObservableCollection<string> SoundSpeedCorrections { get; set; }
        public string SelectedSoundSpeedCorrection { get; set; }
        public bool Amplification { get; set; }
        public bool Inlet { get; set; }
        public string SoundSpeed { get; set; }

        public ICommand StartMeasurementCommand { get; set; }
        public ICommand ValveTestCommand { get; set; }
        public DuMeasurementViewModel(SensorData sensorData) : base(sensorData)
        {
            try
            {
                ResearchTypes = new ObservableCollection<string>
                {
                    Resource.DynamicLevel,
                    Resource.StaticLevel
                };
                StartMeasurementCommand = new Command(StartMeasurementHandler);
                ValveTestCommand = new Command(() => DependencyService.Get<IToast>().Show(Resource.ValveTest));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ddim2MeasurementViewModel constructor");
                throw;
            }
        }

        private async void StartMeasurementHandler(object obj)
        {
            try
            {
                //StartMeasurementCommand = new Command(() => { });
                //if (!ValidateForEmptiness())
                //{
                //    return;
                //}

                //var secondaryParameters = new MeasurementSecondaryParameters(
                //    _sensorData.Name,
                //    Resource.Dynamogram,
                //    SelectedField,
                //    Well,
                //    Bush,
                //    Shop,
                //    BufferPressure,
                //    Comments);

                //var measurementParams = new Ddim2MeasurementStartParameters(
                //    float.Parse(DynPeriod, CultureInfo.InvariantCulture),
                //    int.Parse(ApertNumber),
                //    float.Parse(Imtravel, CultureInfo.InvariantCulture),
                //    GetModelPump(),
                //    secondaryParameters);

                //if (!ValidateMeasurementParameters(measurementParams))
                //{
                //    return;
                //}

                await App.Navigation.PopAsync();
                //await SensorService.Instance.StartMeasurementOnSensor(_sensorData.Id, measurementParams);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartMeasurementHandler Ddim2MeasurementVM");
                throw;
            }
        }

        protected override void InitMeasurementStartParameters()
        {
            throw new NotImplementedException();
        }

        protected override bool ValidateForEmptinessEveryParameter()
        {
            throw new NotImplementedException();
        }

        protected override bool ValidateMeasurementParameters(DuMeasurementStartParameters measurementParameters)
        {
            throw new NotImplementedException();
        }
    }
}
