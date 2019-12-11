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
    public partial class Ddin2MeasurementPage : ContentPage
    {
        public Ddin2MeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModel<Ddin2MeasurementViewModel>(sensorData);
            BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}