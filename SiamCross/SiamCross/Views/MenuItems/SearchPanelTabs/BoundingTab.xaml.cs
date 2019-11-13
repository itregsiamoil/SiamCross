using SiamCross.Models.Scanners;
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
            _viewModel = new ScannerViewModel();
            this.BindingContext = _viewModel;
        }

        private void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ScannedDeviceInfo device = (ScannedDeviceInfo)e.SelectedItem;
            _viewModel.SelectedDevice = device;
            Console.WriteLine(_viewModel.SelectedDevice.Name);
        }
    }
}