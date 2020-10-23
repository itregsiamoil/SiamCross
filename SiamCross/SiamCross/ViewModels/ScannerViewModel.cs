using SiamCross.Models.Scanners;
using SiamCross.Services;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Windows.Input;
using SiamCross.Models;
using SiamCross.Services.Logging;
using SiamCross.AppObjects;
using Autofac;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using SiamCross.Models.USB;

namespace SiamCross.ViewModels
{
    public class ScannerViewModel : IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private IBluetoothScanner _scanner;

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ObservableCollection<ScannedDeviceInfo> ClassicDevices { get; }

        public  ObservableCollection<ScannedDeviceInfo> UsbDevices { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string USBState { get; set; }

        private void OnUsbAttached()
        {
            USBState = Resource.Attached;
            UsbStateChanged?.Invoke(true);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(USBState)));
        }

        private void OnUsbDetached()
        {
            USBState = Resource.Detached;
            UsbStateChanged?.Invoke(false);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(USBState)));
        }

        public ScannerViewModel(IBluetoothScanner scanner)
        {
            if(USBService.Instance.IsUsbConnected)
            {
                OnUsbAttached();              
            }
            else
            {
                OnUsbDetached();              
            }

            MessagingCenter.Subscribe<USBService>(
                this,
                "UsbAttached",
                (sender) => 
                { OnUsbAttached(); });

            MessagingCenter.Subscribe<USBService>(
                this,
                "UsbDetached",
                (sender) => { OnUsbDetached(); });

            _scanner = scanner;
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            ClassicDevices = new ObservableCollection<ScannedDeviceInfo>();
            UsbDevices = new ObservableCollection<ScannedDeviceInfo>();

            _scanner.Received += ScannerReceivedDevice;
            _scanner.ScanTimoutElapsed += ScannerScanTimoutElapsed;
            _scanner.Start();
        }

        public event Action ScanTimeoutElapsed;

        private void ScannerScanTimoutElapsed()
        {
            ScanTimeoutElapsed?.Invoke();
        }

        private void ScannerReceivedDevice(ScannedDeviceInfo dev)
        {
            if (dev.Name == null ||
                dev.BluetoothArgs == null ||
                dev.Name == "")
            {
                return;
            }

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
                case BluetoothType.UsbCustom5:
                    if (!UsbDevices.Contains(dev))
                    {
                        UsbDevices.Add(dev);
                    }
                    break;
                default:
                    break;
            }
        }

        public void StartScan()
        {
            try
            {
                ScannedDevices.Clear();
                ClassicDevices.Clear();
                UsbDevices.Clear();
                _scanner.Start();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartScan" + "\n");
                throw;
            }
        }

        public event Action<bool> UsbStateChanged;
    }
}
