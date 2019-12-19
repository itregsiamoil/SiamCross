using SiamCross.Models;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControlPanelPage : ContentPage
    {
        public ControlPanelPage()
        {
            var vm = new ViewModel<ControlPanelPageViewModel>();
            BindingContext = vm.GetViewModel;
            InitializeComponent();
        }

        private void sensorList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                if (e.Item is SensorData sensorData)
                {
                    if (sensorData.Name.Contains("DDIM"))
                    {
                        App.NavigationPage.Navigation.PushModalAsync(
                            new Ddim2MeasurementPage(sensorData), true);
                    }
                    
                }
            }
        }
    }
}