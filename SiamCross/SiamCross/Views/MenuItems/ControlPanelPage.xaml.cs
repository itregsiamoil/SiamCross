﻿using System;
using System.Linq;
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
                        if (CanOpenMeasurement(sensorData))
                        {
                            if (CanOpenPage(typeof(Ddim2MeasurementPage)))
                            {
                                App.NavigationPage.Navigation.PushAsync(
                                    new Ddim2MeasurementPage(sensorData));
                            }
                        }
                    }
                    else if (sensorData.Name.Contains("DDIN"))
                    {
                        if (CanOpenMeasurement(sensorData))
                        {
                            if (CanOpenPage(typeof(Ddin2MeasurementPage)))
                            {
                                App.NavigationPage.Navigation.PushAsync(
                                    new Ddin2MeasurementPage(sensorData));
                            }
                        }
                    }
                    else if (sensorData.Name.Contains("SIDDOSA3M"))
                    {
                        if (CanOpenMeasurement(sensorData))
                        {
                            if (CanOpenPage(typeof(SiddosA3MMeasurementPage)))
                            {
                                App.NavigationPage.Navigation.PushAsync(
                                    new SiddosA3MMeasurementPage(sensorData));
                            }
                        }
                    }
                }
            }
        }

        
        private bool CanOpenMeasurement(SensorData sensorData)
        {
            bool result = true;
            var sensor = SensorService.Instance.Sensors.SingleOrDefault(
                                s => s.SensorData.Id == sensorData.Id);
            if (sensor != null)
            {
                if (sensor.IsAlive)
                {
                    if (sensor.IsMeasurement)
                        result = false;
                }
                else result = false;
            }

            return result;
        }

        private bool CanOpenModalPage(Type type)
        {
            bool result = false;
            var stack = App.NavigationPage.Navigation.ModalStack;

            if (stack.Count > 0)
            {
                if (stack[stack.Count - 1].GetType() != type)
                {
                    result = true;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        private bool CanOpenPage(Type type)
        {
            var stack = App.NavigationPage.Navigation.NavigationStack;
            if (stack[stack.Count - 1].GetType() != type)
                return true;
            return false;
        }
    }
}