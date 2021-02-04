using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Services.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class ScannerViewModel : IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private readonly IBluetoothScanner _scanner;

        bool _ActiveScan = false;
        public bool ActiveScan
        {
            get => _ActiveScan;
            private set 
            {
                if (value)
                    StartScan();
                else
                    StopScan();
                _ActiveScan = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveScan)));
            }
        }

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; }

        public ObservableCollection<ScannedDeviceInfo> ClassicDevices { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public ScannerViewModel(IBluetoothScanner scanner)
        {
            _scanner = scanner;
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
            ClassicDevices = new ObservableCollection<ScannedDeviceInfo>();

            _scanner.Received += ScannerReceivedDevice;
            _scanner.ScanTimoutElapsed += ScannerScanTimoutElapsed;
            _scanner.Start();
        }

        public event Action ScanTimeoutElapsed;

        private void ScannerScanTimoutElapsed()
        {
            
        }

        private void ScannerReceivedDevice(ScannedDeviceInfo dev)
        {
            /*
            if (dev.Name == null ||
                dev.Id == null ||
                dev.Name == "")
            {
                return;
            }

            if (!IsSiamSensor(dev))
                return;
            */
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

        public bool IsSiamSensor(ScannedDeviceInfo dev)
        {
            return dev.Name.Contains("DDIN")
                   || dev.Name.Contains("DDIM")
                   || dev.Name.Contains("SIDDOSA3M")
                   || dev.Name.Contains("DU");
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
