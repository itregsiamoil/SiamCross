using SiamCross.Models.Tools;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPanelPage : ContentPage
    {
        public SettingsPanelPage()
        {
            var vm = new ViewModel<SettingsViewModel>();
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Settings.Instance.SaveSettings();
        }
    }
}