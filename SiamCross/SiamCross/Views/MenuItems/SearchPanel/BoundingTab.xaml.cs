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
    public partial class BoundingTab : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private readonly ScannerViewModel _vm;
        public BoundingTab()
        {
            InitializeComponent();
            _vm = new ViewModelWrap<ScannerViewModel>().ViewModel;
            _vm.Scanner.ScanStarted += () => { boundedDevicesList.IsRefreshing = true; };
            _vm.Scanner.ScanStoped += () => { boundedDevicesList.IsRefreshing = false; };
            BindingContext = _vm;
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
                _logger.Error(ex, "ItemSelected, creating sensor" + "\n");
            }
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.AppendBonded();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.StopScan();
        }
    }
}