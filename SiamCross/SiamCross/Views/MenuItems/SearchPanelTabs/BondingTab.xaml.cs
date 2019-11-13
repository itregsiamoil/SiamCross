using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanelTabs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BondingTab : ContentView
    {
        public BondingTab()
        {
            var vm = new ViewModel<ScannerViewModel>();
            BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}