using SiamCross.AppObjects;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanelTabs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BoundingTab : ContentPage
    {
        private ScannerViewModel _viewModel;
        public BoundingTab()
        {
            InitializeComponent();
            var vm = new ViewModel<ScannerViewModel>();
            _viewModel = vm.GetViewModel;
            this.BindingContext = _viewModel;
        }


        public void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                SensorService.Instance.AddSensor(
                    SensorFactory.CreateSensor((ScannedDeviceInfo)e.SelectedItem));
            }
        }
    }
}