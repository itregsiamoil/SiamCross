using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControlPanelPage : ContentPage
    {
        public ControlPanelPage()
        {
            ViewModelWrap<ControlPanelPageViewModel> vm = new ViewModelWrap<ControlPanelPageViewModel>();
            BindingContext = vm.ViewModel;
            InitializeComponent();
        }

        static protected async Task RequestEnableBuetooth()
        {
            IPhyInterface defaultAdapter = FactoryBt2.GetCurent();

            if (!defaultAdapter.IsEnbaled)
            {
                bool result = await Application.Current.MainPage.DisplayAlert(
                    Resource.BluetoothIsDisable,
                    Resource.EnableBluetooth,
                    Resource.YesButton,
                    Resource.NotButton);
                if (result)
                {
                    defaultAdapter.Enable();
                }
            }
        }
        static protected async void StartRequestEnableBuetooth()
        {
            await Task.Run(RequestEnableBuetooth);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartRequestEnableBuetooth();
        }


    }
}