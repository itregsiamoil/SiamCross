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
            var vm = new ViewModelWrap<AddFieldViewModel>();
            this.BindingContext = vm.ViewModel;
            InitializeComponent();
        }
    }
}