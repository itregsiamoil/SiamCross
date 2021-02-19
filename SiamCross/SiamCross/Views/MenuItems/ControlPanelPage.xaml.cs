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

        private void SensorList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                if (e.Item is ISensor dvc)
                {
                    //dvc.Activate = !dvc.Activate;
                }
            }
        }

        protected async Task RequestEnableBuetooth()
        {
            IPhyInterface defaultAdapter = FactoryBt2.GetCurent();

            if (!defaultAdapter.IsEnbaled)
            {
                bool result = await DisplayAlert(
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
        protected async void StartRequestEnableBuetooth()
        {
            Task checkBuetooth = Task.Run(RequestEnableBuetooth);
            await checkBuetooth;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            StartRequestEnableBuetooth();
        }


    }
}