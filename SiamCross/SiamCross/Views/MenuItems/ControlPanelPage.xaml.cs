using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControlPanelPage : BaseContentPage
    {
        ControlPanelPageViewModel _vm;
        public ControlPanelPage()
        {
            InitializeComponent();
            ControlPanelPageViewModel vm = new ControlPanelPageViewModel();
            _vm = vm;
            BindingContext = _vm;
        }

        protected async void RequestEnableBuetooth()
        {
            while (0 > this.Height)
                await Task.Delay(1000);
            IPhyInterface defaultAdapter = FactoryBt2.GetCurent();
            if (null == defaultAdapter)
            {
                await this.DisplayToastAsync("There are no Bluetooth module");
                return;
            }
            if (defaultAdapter.IsEnbaled)
                return;
            bool result = await Application.Current.MainPage.DisplayAlert(
                            Resource.BluetoothIsDisable,
                            Resource.EnableBluetooth,
                            Resource.YesButton,
                            Resource.NotButton);
            if (!result)
                return;
            defaultAdapter.Enable();
        }

        protected override void OnAppearing()
        {
            BindingContext = _vm;
            base.OnAppearing();
            RequestEnableBuetooth();
            _vm.EnableQickInfoAll();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.DisableQickInfoAll();
            BindingContext = null;
        }


        protected Task ExecuteExit()
        {
            //while (0 > this.Height)
            //    await Task.Delay(1000);
            //System.Environment.Exit(0);
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            System.Diagnostics.Process.GetCurrentProcess().Close();
            return Task.CompletedTask;
        }
        protected async void RequestExit()
        {
            var options = new SnackBarOptions();
            var appRes = Application.Current.Resources;

            Color txtColor = Color.Black;
            Color acentColor = Color.Red;
            if (appRes.TryGetValue("colorAccentLight", out object obj_acc_color))
                if (obj_acc_color is Color resAcentColor)
                    acentColor = resAcentColor;
            //if (appRes.TryGetValue("colorPrimaryLight", out object obj_bg_color))
            //    if (obj_bg_color is Color bgColor)
            //        options.BackgroundColor = bgColor;
            options.BackgroundColor = acentColor;

            options.MessageOptions.Foreground = txtColor;
            options.MessageOptions.Message = "Close app?";

            options.Actions = new List<SnackBarActionOptions>
            {
                    new SnackBarActionOptions
                    {
                        ForegroundColor = txtColor,
                        BackgroundColor = acentColor,
                        Text = Resource.YesButton,
                        Action = () =>
                        {
                            ExecuteExit();
                            return Task.CompletedTask;
                        }
                    }
            };
            bool _ExitCmd = await this.DisplaySnackBarAsync(options);
        }
        protected override bool OnBackButtonPressed()
        {
            RequestExit();
            return true;
        }


    }
}