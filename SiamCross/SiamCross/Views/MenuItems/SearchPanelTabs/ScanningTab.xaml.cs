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

namespace SiamCross.Views.MenuItems.SearchPanelTabs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningTab : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private ScannerViewModel _vm;
        public ScanningTab()
        {
            InitializeComponent();
            _vm = new ViewModel<ScannerViewModel>().GetViewModel;
            this.BindingContext = _vm;
            scannedDevicesList.RefreshCommand = new Command(() =>
            {
                try
                {
                    _vm.StartScan();
                    scannedDevicesList.IsRefreshing = false;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "RefreshCommand");
                }
            });
        }

        public void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.SelectedItem != null)
                {
                    SensorService.Instance.AddSensor((ScannedDeviceInfo)e.SelectedItem);
                    App.NavigationPage.Navigation.PopToRootAsync();
                    App.MenuIsPresented = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ItemSelected (creating sensor)");
            }
        }
    }
}