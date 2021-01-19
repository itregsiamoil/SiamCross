using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddFieldPage : ContentPage
    {
        public AddFieldPage()
        {
            ViewModelWrap<AddFieldViewModel> vm = new ViewModelWrap<AddFieldViewModel>();
            BindingContext = vm.ViewModel;
            InitializeComponent();
        }
    }
}