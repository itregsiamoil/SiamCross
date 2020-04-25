﻿using SiamCross.Models;
using SiamCross.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Ddim2MeasurementPage : ContentPage
    {
        private Stopwatch _stopwatch;
        private Ddim2MeasurementViewModel _vm;
        public Ddim2MeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModelWrap<Ddim2MeasurementViewModel>(sensorData);
            _vm = vm.ViewModel;
            BindingContext = _vm;
            _stopwatch = new Stopwatch();
            InitializeComponent();
        }

        private void StopwatchButton_Clicked(object sender, EventArgs e)
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
            }
            else
            {
                _stopwatch.Reset();
                _stopwatch.Start();

                Xamarin.Forms.Device.StartTimer(TimeSpan.FromMilliseconds(100),
                    () =>
                    {
                        DynPeriodEntry.Text = _stopwatch.Elapsed.TotalSeconds.ToString(
                            "0.000", CultureInfo.InvariantCulture);

                        if (!_stopwatch.IsRunning)
                            return false;
                        else 
                            return true;
                    });
            }
        }
    }
}