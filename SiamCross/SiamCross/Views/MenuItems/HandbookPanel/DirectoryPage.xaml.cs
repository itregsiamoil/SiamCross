using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.HandbookPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DirectoryPage : ContentPage
    {
        public DirectoryPage()
        {
            ViewModelWrap<DirectoryViewModel> vm = new ViewModelWrap<DirectoryViewModel>();
            BindingContext = vm.ViewModel;
            InitializeComponent();
        }
    }
}