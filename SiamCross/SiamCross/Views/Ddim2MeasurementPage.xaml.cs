using SiamCross.Models;
using SiamCross.ViewModels;
using SiamCross.Views.MenuItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Ddim2MeasurementPage : ContentPage
    {
        private Stopwatch _stopwatch;
        private Ddim2MeasurementViewModel _vm;
        private StopwatchState _stopwatchState;
        public Ddim2MeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModel<Ddim2MeasurementViewModel>(sensorData);
            _vm = vm.GetViewModel;
            BindingContext = _vm;
            _stopwatch = new Stopwatch();
            _stopwatchState = StopwatchState.NeverStarted;
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

                Device.StartTimer(TimeSpan.FromMilliseconds(100),
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

        private enum StopwatchState
        {
            NeverStarted,
            Started,
            Stoped
        }
    }
}