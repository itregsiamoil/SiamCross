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
            if (Settings.Instance.NeedAuthorization)
            {
                AuthCheckBox.IsChecked = true;
            }
            SwitchFieldsEnability();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await Settings.Instance.SaveSettings();
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            SwitchFieldsEnability();
            _vm.GetViewModel.NeedAuthorization = e.Value;
        }

        private void SwitchFieldsEnability()
        {
            UsernameEntry.IsEnabled = AuthCheckBox.IsChecked;
            PasswordEntry.IsEnabled = AuthCheckBox.IsChecked;
        }
    }
}