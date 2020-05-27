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
using SiamCross.Models;
using SiamCross.Droid.Models;
using System.Threading;
using System.Threading.Tasks;

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

        public ScannerViewModel(IBluetoothScanner scanner)
        {
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
                _scanner.Start();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartScan" + "\n");
                throw;
            }
        }
    }
}
