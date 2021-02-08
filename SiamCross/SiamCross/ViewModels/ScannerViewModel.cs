using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class ScannerViewModel : IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private readonly IBluetoothScanner _scanner;

        public IBluetoothScanner Scanner => _scanner;


        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ObservableCollection<ScannedDeviceInfo> ClassicDevices { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand RefreshCommand { get; }
        public ICommand SelectItemCommand { get; }
        public ICommand StartStopScanCommand { get; }

        public ScannerViewModel(IBluetoothScanner scanner)
        {
            _scanner = scanner;
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            ClassicDevices = new ObservableCollection<ScannedDeviceInfo>();

            _scanner.Received += ScannerReceivedDevice;
            _scanner.ScanStoped += ScannerScanTimoutElapsed;

            RefreshCommand = new Command(StartScan);
            SelectItemCommand = new Command(SelectItem);
            StartStopScanCommand = new Command(StartStopScan);
            StartScan();
        }

        private void ScannerScanTimoutElapsed()
        {
        }

        private void ScannerReceivedDevice(ScannedDeviceInfo dev)
        {
            switch (dev.BluetoothType)
            {
                case BluetoothType.Classic:
                    if (!ClassicDevices.Contains(dev))
                    {
                        ClassicDevices.Add(dev);
                    }
                    break;
                case BluetoothType.Le:
                    if (!ScannedDevices.Contains(dev))
                    {
                        ScannedDevices.Add(dev);
                    }
                    break;
                default:
                    break;
            }
        }



        private void SelectItem(object obj)
        {
            if (obj is ScannedDeviceInfo dev)
                SelectItem(dev);
        }
        public void SelectItem(ScannedDeviceInfo item)
        {
            try
            {
                if (item == null)
                    return;
                SensorService.Instance.AddSensor(item);
                App.NavigationPage.Navigation.PopToRootAsync();
                App.MenuIsPresented = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ItemSelected (creating sensor)" + "\n");
            }
        }

        public void AppendBonded()
        {
            _scanner.StartBounded();
        }

        private void StartStopScan(object obj)
        {
            if (_scanner.ActiveScan)
                StopScan();
            else
                StartScan();
        }

        public void StartScan()
        {
            try
            {
                ScannedDevices.Clear();
                ClassicDevices.Clear();
                _scanner.Start();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartScan" + "\n");
                throw;
            }
        }

        public void StopScan()
        {
            try
            {
                _scanner.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StopScan" + "\n");
                throw;
            }
        }
    }
}
