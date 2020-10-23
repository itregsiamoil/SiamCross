using SiamCross.Models;
using SiamCross.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Ddin2MeasurementPage : ContentPage
    {
        private Stopwatch _stopwatch;

        public Ddin2MeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModelWrap<Ddin2MeasurementViewModel>(sensorData);
            BindingContext = vm.ViewModel;
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
                        double pump_rate = 60.0 / _stopwatch.Elapsed.TotalSeconds;
                        PumpRateEntry.Text = pump_rate.ToString("N3", CultureInfo.InvariantCulture);

                        if (!_stopwatch.IsRunning)
                            return false;
                        else
                            return true;
                    });
            }
        }
    }
}