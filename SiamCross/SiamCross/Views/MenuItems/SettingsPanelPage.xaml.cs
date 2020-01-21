using SiamCross.Models.Tools;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPanelPage : ContentPage
    {
        private readonly ViewModel<SettingsViewModel> _vm;
        public SettingsPanelPage()
        {
            _vm = new ViewModel<SettingsViewModel>();
            this.BindingContext = _vm.GetViewModel;
            InitializeComponent();
            UsernameEntry.IsEnabled = false;
            PasswordEntry.IsEnabled = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Settings.Instance.SaveSettings();
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UsernameEntry.IsEnabled = true;
            PasswordEntry.IsEnabled = true;

            _vm.GetViewModel.NeedAuthorization = e.Value;
        }
    }
}