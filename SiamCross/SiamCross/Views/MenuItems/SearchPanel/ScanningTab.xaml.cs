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
        private readonly ScannerViewModel _vm;
        public ScanningTab()
        {
            InitializeComponent();
            _vm = new ViewModelWrap<ScannerViewModel>().ViewModel;
            _vm.Scanner.ScanStarted += () => { scannedDevicesList.IsRefreshing = true; };
            _vm.Scanner.ScanStoped += () => { scannedDevicesList.IsRefreshing = false; };
            BindingContext = _vm;
            
        }
        /*
        public void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            _vm.SelectItem(e.SelectedItem)
        }
        */
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