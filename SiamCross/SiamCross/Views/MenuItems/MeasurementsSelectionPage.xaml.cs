using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsSelectionPage : ContentPage
    {
        private readonly ViewModelWrap<MeasurementsSelectionViewModel> _vm;

        public MeasurementsSelectionPage(ObservableCollection<MeasurementView> measurements)
        {
            _vm = new ViewModelWrap<MeasurementsSelectionViewModel>(measurements);
            this.BindingContext = _vm.ViewModel;
            InitializeComponent();
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            _vm.ViewModel.SaveMeasurements(sender);
        }
    }
}