using SiamCross.Models;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DuMeasurementPage : ContentPage
    {
        private DuMeasurementViewModel _vm;
        public DuMeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModel<DuMeasurementViewModel>(sensorData);
            _vm = vm.GetViewModel;
            this.BindingContext = _vm;
            InitializeComponent();
        }

        private void AmplificationCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            _vm.Amplification = e.Value;
        }
        
        private void InletCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            _vm.Inlet = e.Value;
        }
    }
}