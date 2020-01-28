using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanelTabs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningTab : ContentPage
    {
        private ScannerViewModel _vm;
        public ScanningTab()
        {
            InitializeComponent();
            _vm = new ViewModel<ScannerViewModel>().GetViewModel;
            this.BindingContext = _vm;
            scannedDevicesList.RefreshCommand = new Command(() =>
            {
                _vm.StartScan();
                scannedDevicesList.IsRefreshing = false;
            });
        }

        public void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                SensorService.Instance.AddSensor((ScannedDeviceInfo)e.SelectedItem);
                App.NavigationPage.Navigation.PopToRootAsync();
                App.MenuIsPresented = false;
            }
        }
    }
}