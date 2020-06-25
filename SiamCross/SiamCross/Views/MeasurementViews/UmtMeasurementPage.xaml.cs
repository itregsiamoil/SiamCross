using SiamCross.Models;
using SiamCross.ViewModels.MeasurementViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MeasurementViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UmtMeasurementPage : ContentPage
    {
        private UmtMeasurementViewModel _vm;
        public UmtMeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModelWrap<UmtMeasurementViewModel>(sensorData);
            _vm = vm.ViewModel;
            BindingContext = _vm;
            InitializeComponent();
        }

        private void TemperatureMeasurementCheckboxChanged(object sender, CheckedChangedEventArgs e)
        {
            _vm.IsTemperatureMesure = e.Value;
        }
    }
}