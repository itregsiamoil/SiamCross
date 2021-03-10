using SiamCross.Models;
using SiamCross.Models.Scanners;
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
        private readonly Stopwatch _stopwatch;

        public Ddin2MeasurementPage(ScannedDeviceInfo sensorData)
        {
            ViewModelWrap<Ddin2MeasurementViewModel> vm = new ViewModelWrap<Ddin2MeasurementViewModel>(sensorData);
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
                        double dyn_period = _stopwatch.Elapsed.TotalSeconds;
                        txtDynPeriod.Text = dyn_period.ToString("N3", CultureInfo.InvariantCulture);

                        if (!_stopwatch.IsRunning)
                            return false;
                        else
                            return true;
                    });
            }
        }
    }
}