using SiamCross.ViewModels;
using System;
using System.Collections.ObjectModel;
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
            BindingContext = _vm.ViewModel;
            InitializeComponent();
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            _vm.ViewModel.SaveMeasurements(sender);
        }
    }
}