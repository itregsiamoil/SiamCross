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
    public partial class AddFieldPage : ContentPage
    {
        public AddFieldPage()
        {
            var vm = new ViewModel<AddFieldViewModel>();
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}