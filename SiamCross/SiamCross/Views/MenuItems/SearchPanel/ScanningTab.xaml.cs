using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningTab : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private readonly ScannerViewModel _vm;
        public ScanningTab()
        {
            InitializeComponent();
            _vm = new ViewModelWrap<ScannerViewModel>().ViewModel;
            BindingContext = _vm;
            _vm.ScanTimeoutElapsed += () => ScanAnimation.IsRunning = false;
            scannedDevicesList.RefreshCommand = new Command(() =>
            {
                try
                {
                    _vm.StartScan();
                    ScanAnimation.IsRunning = true;
                    scannedDevicesList.IsRefreshing = false;
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
                        SensorService.Instance.AddSensor(dev);
                        App.NavigationPage.Navigation.PopToRootAsync();
                        App.MenuIsPresented = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ItemSelected (creating sensor)" + "\n");
            }
        }
    }
}