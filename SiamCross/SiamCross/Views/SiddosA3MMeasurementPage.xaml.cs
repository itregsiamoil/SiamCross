using SiamCross.Models;
using SiamCross.ViewModels;
using SiamCross.Views.MenuItems;
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
    public partial class SiddosA3MMeasurementPage : ContentPage
    {
        public SiddosA3MMeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModel<SiddosA3MMeasurementViewModel>(sensorData);
            BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}