using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsSelectionPage : ContentPage
    {
        public MeasurementsSelectionPage(ObservableCollection<MeasurementView> measurements)
        {
            var vm = new ViewModel<MeasurementsSelectionViewModel>(measurements);
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}