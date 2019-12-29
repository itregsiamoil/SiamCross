using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MeasurementsPage : ContentPage
    {
        public MeasurementsPage()
        {
            var vm = new ViewModel<MeasurementsViewModel>();
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}