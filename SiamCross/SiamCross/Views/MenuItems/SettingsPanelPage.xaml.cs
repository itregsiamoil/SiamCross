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
            InitializeComponent();
            _vm = new ViewModelWrap<SettingsViewModel>();
            BindingContext = _vm.ViewModel;
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

    }
}