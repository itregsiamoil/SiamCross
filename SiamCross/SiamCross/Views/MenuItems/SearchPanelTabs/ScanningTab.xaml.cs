using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanelTabs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningTab : ContentPage
    {
        public ScanningTab()
        {
            InitializeComponent();
            this.BindingContext = new ScannerViewModel();
        }
    }
}