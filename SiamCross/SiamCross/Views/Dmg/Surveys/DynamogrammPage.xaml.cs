using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DynamogrammPage : ContentPage
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public DynamogrammPage()
        {
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
                        txtDynPeriod.Text = dyn_period.ToString("N3");

                        if (!_stopwatch.IsRunning)
                            return false;
                        else
                            return true;
                    });
            }
        }
    }
}