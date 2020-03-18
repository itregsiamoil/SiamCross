using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DirectoryPage : ContentPage
    {
        public DirectoryPage()
        {
            var vm = new ViewModel<DirectoryViewModel>();
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}