﻿using System;
using System.Linq;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Services;
using SiamCross.ViewModels;
using Autofac;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
using System.Threading;
using SiamCross.Views.MeasurementViews;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControlPanelPage : ContentPage
    {
        public ControlPanelPage()
        {
            var vm = new ViewModelWrap<ControlPanelPageViewModel>();
            BindingContext = vm.ViewModel;
            InitializeComponent();
            var checkBuetooth = new Thread(async () =>
            {
                var defaultAdapter = AppContainer.Container.Resolve<IDefaultAdapter>();
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
            });
            checkBuetooth.Start();
        }

        private void sensorList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null)
            {
                if (e.Item is ISensor dvc)
                {
                    //dvc.Activate = !dvc.Activate;
                }
            }
        }


    }
}