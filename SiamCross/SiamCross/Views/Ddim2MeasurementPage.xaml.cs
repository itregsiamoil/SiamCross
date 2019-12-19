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
    public partial class Ddim2MeasurementPage : ContentPage
    {
        public Ddim2MeasurementPage(SensorData sensorData)
        {
            var vm = new ViewModel<Ddim2MeasurementViewModel>(sensorData);
            BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}