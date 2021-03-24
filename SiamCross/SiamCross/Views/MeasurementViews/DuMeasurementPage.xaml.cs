﻿using SiamCross.Models.Sensors;
using SiamCross.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DuMeasurementPage : ContentPage
    {
        private readonly DuMeasurementViewModel _vm;
        public DuMeasurementPage(ISensor sensor)
        {
            InitializeComponent();
            ViewModelWrap<DuMeasurementViewModel> vm = new ViewModelWrap<DuMeasurementViewModel>(sensor);
            _vm = vm.ViewModel;
            BindingContext = _vm;
        }

        private void AmplificationCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            //_vm.Amplification = e.Value;
        }

        private void InletCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            //_vm.Inlet = e.Value;
        }
    }
}