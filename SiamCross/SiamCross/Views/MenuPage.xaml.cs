using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static SiamCross.ViewModels.MenuPageViewModel;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        private readonly MenuPageViewModel _vm;
        public MenuPage()
        {
            _vm = new MenuPageViewModel();
            BindingContext = _vm;
            //this.Icon = "yourHamburgerIcon.png"; //only neeeded for ios
            InitializeComponent();
        }

        private void MenuListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                ((MenuPageItem)e.SelectedItem).Command?.Execute(sender);
            }
        }

        private void MenuListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                ((MenuPageItem)e.Item).Command?.Execute(sender);
            }
        }
    }
}