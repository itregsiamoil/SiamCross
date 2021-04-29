using SiamCross.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.Dua
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DuaStoragePage : ContentPage
    {
        public DuaStoragePage()
        {
            InitializeComponent();

        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
            if (BindingContext is BasePageVM vm)
            {
                vm.ChangeNotify(nameof(BasePageVM.IsLandscape));
                vm.ChangeNotify(nameof(BasePageVM.IsPortrait));
            }
        }
    }
}
