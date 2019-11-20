﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanelTabs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanningTab : ContentPage
    {
        public ScanningTab()
        {
            InitializeComponent();
            var vm = new ViewModel<ScannerViewModel>();
            this.BindingContext = vm.GetViewModel;
        }

        public void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                SensorService.Instance.AddSensor(
                    SensorFactory.CreateSensor((ScannedDeviceInfo)e.SelectedItem));
            }
        }
    }
}