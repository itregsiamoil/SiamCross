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

        private ScannerViewModel _viewModel;
        public BoundingTab()
        {
            InitializeComponent();
            var vm = new ViewModelWrap<ScannerViewModel>();
            _viewModel = vm.ViewModel;
            this.BindingContext = _viewModel;
            boundedDevicesList.RefreshCommand = new Command(() =>
            {
                try
                {
                    _viewModel.StartScan();
                    boundedDevicesList.IsRefreshing = false;
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
                _logger.Error(ex, "ItemSelected, creating sensor");
            }
        }
    }
}