using NLog;
using SiamCross.Models.Scanners;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UsbTab : ContentPage
    {
        private static readonly Logger _logger = DependencyService.Get<ILogManager>().GetLog();

        private readonly ScannerViewModel _viewModel;
        public UsbTab()
        {
            InitializeComponent();
            ScannerViewModel vm = null;// new ScannerViewModel();

            _viewModel = vm;
            //_viewModel.ScanTimeoutElapsed += () => ScanAnimation.IsRunning = false;
            BindingContext = _viewModel;
            usbDevicesList.RefreshCommand = new Command(() =>
            {
                try
                {
                    _viewModel.StartScan();
                    ScanAnimation.IsRunning = true;
                    usbDevicesList.IsRefreshing = false;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "RefreshCommand" + "\n");
                }
            });
        }


        public void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.SelectedItem != null)
                {
                    if (e.SelectedItem is ScannedDeviceInfo dev)
                    {
                        //await SensorService.Instance.AddSensorAsync(dev);
                        //await App.NavigationPage.Navigation.PopToRootAsync();
                        //App.MenuIsPresented = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ItemSelected, creating sensor" + "\n");
            }
        }

    }
}