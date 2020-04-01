using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Tools;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPanelPage : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private readonly ViewModelWrap<SettingsViewModel> _vm;
        public SettingsPanelPage()
        {
            _vm = new ViewModelWrap<SettingsViewModel>();
            this.BindingContext = _vm.ViewModel;
            InitializeComponent();
            if (Settings.Instance.IsNeedAuthorization)
            {
                AuthCheckBox.IsChecked = true;
            }
            SwitchFieldsEnability();
        }

        protected override async void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                await Settings.Instance.SaveSettings();
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "OnDisappearing" + "\n");
                throw;
            }
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            SwitchFieldsEnability();
            _vm.ViewModel.NeedAuthorization = e.Value;
        }

        private void SwitchFieldsEnability()
        {
            UsernameEntry.IsEnabled = AuthCheckBox.IsChecked;
            PasswordEntry.IsEnabled = AuthCheckBox.IsChecked;
        }
    }
}