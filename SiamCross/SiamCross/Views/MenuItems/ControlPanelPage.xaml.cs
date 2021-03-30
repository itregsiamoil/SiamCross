using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.ViewModels;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControlPanelPage : ContentPage
    {
        ControlPanelPageViewModel _vm;
        public ControlPanelPage()
        {
            InitializeComponent();
            ViewModelWrap<ControlPanelPageViewModel> vm = new ViewModelWrap<ControlPanelPageViewModel>();
            _vm = vm.ViewModel;
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
            base.OnAppearing();
            RequestEnableBuetooth();
            _vm.EnableQickInfoAll();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.DisableQickInfoAll();
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
            bool _ExitCmd = await this.DisplaySnackBarAsync("Exit app?", "Yes", ExecuteExit);
        }
        protected override bool OnBackButtonPressed()
        {
            RequestExit();
            return true;
        }


    }
}