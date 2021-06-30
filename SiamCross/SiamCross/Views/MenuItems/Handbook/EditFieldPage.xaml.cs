using SiamCross.Services;
using SiamCross.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditFieldPage : ContentPage
    {
        public EditFieldPage(FieldItem field = null)
        {
            var vm = new EditFieldVM(field);
            BindingContext = vm;
            InitializeComponent();
        }
    }
}