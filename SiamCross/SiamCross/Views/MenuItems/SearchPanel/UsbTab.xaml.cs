﻿using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.Services.Logging;
using SiamCross.ViewModels;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems.SearchPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UsbTab : ContentPage
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private ScannerViewModel _viewModel;
        public UsbTab()
        {
            InitializeComponent();
            var vm = new ViewModelWrap<ScannerViewModel>();
            
            _viewModel = vm.ViewModel;
            _viewModel.ScanTimeoutElapsed += () => ScanAnimation.IsRunning = false;
            _viewModel.UsbStateChanged += OnUsbStateChanged;
            this.BindingContext = _viewModel;
            usbDevicesList.RefreshCommand = new Command(() =>
            {
                try
                {
                    _viewModel.StartScan();
                    ScanAnimation.IsRunning = true;
                    usbDevicesList.IsRefreshing = false;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "RefreshCommand" + "\n");
                }
            });
        }


        public void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                if (e.SelectedItem != null)
                {
                    if (e.SelectedItem is ScannedDeviceInfo dev)
                    {
                        SensorService.Instance.AddSensor(dev);
                        App.NavigationPage.Navigation.PopToRootAsync();
                        App.MenuIsPresented = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ItemSelected, creating sensor" + "\n");
            }
        }

        private void OnUsbStateChanged(bool state)
        {
            usbStateLabel.TextColor = state ? Color.Green : Color.Red;
        }
    }
}