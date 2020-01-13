﻿using System.Linq;
using SiamCross.Models;
using SiamCross.Services;
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
                        var sensor =
                            SensorService.Instance.Sensors.SingleOrDefault(
                                s => s.SensorData.Id == sensorData.Id 
                                     );
                        App.NavigationPage.Navigation.PushModalAsync(
                            new Ddim2MeasurementPage(sensorData), true);
                    }
                    else if (sensorData.Name.Contains("DDIN"))
                    {
                        App.NavigationPage.Navigation.PushModalAsync(
                            new Ddin2MeasurementPage(sensorData), true);
                    }
                    
                }
            }
        }
    }
}