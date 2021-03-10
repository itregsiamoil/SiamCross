using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DuMeasurementPage : ContentPage
    {
        private readonly DuMeasurementViewModel _vm;
        public DuMeasurementPage(ScannedDeviceInfo sensorData)
        {
            ViewModelWrap<DuMeasurementViewModel> vm = new ViewModelWrap<DuMeasurementViewModel>(sensorData);
            _vm = vm.ViewModel;
            BindingContext = _vm;
            InitializeComponent();
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