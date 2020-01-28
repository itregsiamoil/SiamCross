using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SiamCross.ViewModels.MenuPageViewModel;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        private MenuPageViewModel _vm;
        public MenuPage()
        {
            _vm = new MenuPageViewModel();
            BindingContext = _vm;
            //this.Icon = "yourHamburgerIcon.png"; //only neeeded for ios
            InitializeComponent();
        }

        private void menuListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                ((MenuPageItem)e.SelectedItem).Command?.Execute(sender);
            }
        }
    }
}