using Android.Bluetooth;
using SiamCross.Droid.Models;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using BLE = Android.Bluetooth.LE;

[assembly: Dependency(typeof(BluetoothScannerAndroid))]
namespace SiamCross.Droid.Models
{
    public class BluetoothScanReceiver : BLE.ScanCallback
    {
        public override void OnBatchScanResults(IList<BLE.ScanResult> results)
        {
            if (null == ActionOnScanResult)
                return;
            foreach (BLE.ScanResult res in results)
                ActionOnScanResult?.Invoke(res);
        }
        public override void OnScanFailed(BLE.ScanFailure errorCode)
        {
            ActionOnScanFailed?.Invoke(errorCode);
        }
        public override void OnScanResult(BLE.ScanCallbackType callbackType, BLE.ScanResult result)
        {
            ActionOnScanResult?.Invoke(result);
        }
        public event Action<BLE.ScanFailure> ActionOnScanFailed;
        public event Action<BLE.ScanResult> ActionOnScanResult;

    }


    [Android.Runtime.Preserve(AllMembers = true)]
    public class BluetoothScannerAndroid : IBluetoothScanner, INotifyPropertyChanged
    {
        readonly BluetoothScanReceiver _receiver = new BluetoothScanReceiver();
        static readonly BluetoothAdapter _bt_adapter = BluetoothAdapter.DefaultAdapter;
        readonly BLE.BluetoothLeScanner _ble_scanner = _bt_adapter.BluetoothLeScanner;

        public event PropertyChangedEventHandler PropertyChanged;

        bool _ActiveScan = false;
        public bool ActiveScan
        {
            get => _ActiveScan;
            private set
            {
                _ActiveScan = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveScan)));
            }
        }

        string  _ScanString;
        public string ScanString
        {
            get => _ScanString;
            private set
            {
                _ScanString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScanString)));
            }
        }

        public int ScanTimeout { get; set; }
        public bool IsFilterEnabled { get; set; }

        public event Action<ScannedDeviceInfo> Received;
        public event Action ScanStoped;
        public event Action ScanStarted;

        public BluetoothScannerAndroid()
        {
            ScanTimeout = 30000;
            IsFilterEnabled = false;

            _receiver.ActionOnScanResult += OnScanResult;
        }
        public async void Start()
        {
            StartBounded();
            await StartScanLeAsync();
        }
        public void Stop()
        {
            StopScanLeAsync();
        }

        public void OnScanResult(BLE.ScanResult result)
        {
            var sd = new ScannedDeviceInfo
            {
                BluetoothType = BluetoothType.Le,
                Name = result.Device.Name,
                Mac = result.Device.Address,
                Id = MacToGuid(result.Device.Address),
                PrimaryPhy = ((BluetoothPhy)result.PrimaryPhy).ToString(),
                SecondaryPhy = ((BluetoothPhy)result.SecondaryPhy).ToString(),
                Rssi = result.Rssi.ToString(),
                PeriodicAdvertisingInterval = result.PeriodicAdvertisingInterval.ToString(),
                IsLegacy = result.IsLegacy.ToString(),
                IsConnectable = result.IsConnectable.ToString(),
                TxPower = ((BLE.AdvertiseTxPower)result.TxPower).ToString(),
                BondState = ((Bond)result.Device.BondState).ToString()
            };

            DoNotifyDevice(sd);
        }


        async Task StartScanLeAsync()
        {
            ScanStarted?.Invoke();
            var b = new BLE.ScanSettings.Builder();
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                b.SetPhy((BluetoothPhy)255);
                b.SetLegacy(false);
            }
            else
            {
            }
            b.SetCallbackType(BLE.ScanCallbackType.AllMatches);//android bug BLE.ScanCallbackType.FirstMatch| 
            b.SetMatchMode(BLE.BluetoothScanMatchMode.Aggressive);
            b.SetNumOfMatches(1);
            b.SetScanMode(BLE.ScanMode.LowLatency);
            b.SetReportDelay(0);
            BLE.ScanSettings st = b.Build();
            _ble_scanner.StartScan(null, st, _receiver);
            string info = SiamCross.Resource.SearchTitle + " BLE ... ";
            StartScanInfo(info, ScanTimeout/1000);
            await Task.Delay(ScanTimeout);
            StopScanLeAsync();
        }
        void StopScanLeAsync()
        {
            _ble_scanner.StopScan(_receiver);
            _ble_scanner.FlushPendingScanResults(_receiver);
            ClearScanInfo();
            ScanStoped?.Invoke();
        }

        public void StartBounded()
        {
            ICollection<BluetoothDevice> devices = BluetoothAdapter.DefaultAdapter.BondedDevices;

            foreach (BluetoothDevice device in devices)
            {
                BluetoothType bluetoothType = BluetoothType.Le;
                switch (device.Type)
                {
                    case BluetoothDeviceType.Classic:
                        bluetoothType = BluetoothType.Classic;
                        break;
                    case BluetoothDeviceType.Le:
                        bluetoothType = BluetoothType.Le;
                        break;
                }
                ScannedDeviceInfo sd = new ScannedDeviceInfo
                {
                    Name = device.Name,
                    Mac = device.Address,
                    Id = MacToGuid(device.Address),
                    BluetoothType = bluetoothType,
                    BondState = device.BondState.ToString()
                };
                DoNotifyDevice(sd);
            }
        }


        #region over_PluginBLE
        /*
        using Plugin.BLE;
        using Plugin.BLE.Abstractions.Contracts;
        using ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode;
        using DeviceEventArgs = Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs;

        private IAdapter _adapter;
        private IBluetoothLE _bluetoothBLE;
        void InitScanner_over_PluginBLE()
        {
            _bluetoothBLE = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _adapter.ScanTimeout = ScanTimeout;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.ScanTimeoutElapsed += ScannerLeTimoutElapsed;
            _adapter.DeviceDiscovered += ScannerLeDeviceFind;
        }
        private void ScannerLeTimoutElapsed(object obj, EventArgs e)
        {
            ClearScanInfo();
            ScanStoped?.Invoke();
        }
        private void ScannerLeDeviceFind(object obj, DeviceEventArgs a)
        {
            if (obj == null || a == null || a.Device == null || a.Device.Name == null)
                return;
            ScannedDeviceInfo sd = new ScannedDeviceInfo
            {
                Name = a.Device.Name,
                Mac = a.Device.Id.ToString(),
                Id = a.Device.Id,
                BluetoothType = BluetoothType.Le
            };
            Received?.Invoke(sd);
            System.Diagnostics.Debug.WriteLine("Finded device" + a.Device.Name);
        }
        private async void StartScanLeAsync_over_PluginBLE()
        {
            if (_bluetoothBLE.State == BluetoothState.Off)
                return;
            if (_bluetoothBLE.Adapter.IsScanning)
                return;
            string info = SiamCross.Resource.SearchTitle + " BLE ... ";
            int timeout = _adapter.ScanTimeout / 1000;
            StartScanInfo(info, timeout);
            await _adapter.StartScanningForDevicesAsync();
        }
        private async void StopScanLeAsync_over_PluginBLE()
        {
            await _adapter.StopScanningForDevicesAsync();
        }
        */
        #endregion

        private void StartScanInfo(string info, int timeout)
        {
            ActiveScan = true;
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!ActiveScan)
                {
                    ClearScanInfo();
                    return false;
                }
                timeout--;
                ScanString = info + timeout.ToString();
                return (timeout > 1);
            });
        }
        private void ClearScanInfo()
        {
            ActiveScan = false;
            ScanString = "";
        }

        private void DoNotifyDevice(ScannedDeviceInfo sd)
        {
            if (IsFilterEnabled)
            {
                if (IsEmptyDevice(sd))
                    return;
                if (!IsSiamSensor(sd))
                    return;
            }
            Received?.Invoke(sd);
        }
        static public bool IsEmptyDevice(ScannedDeviceInfo dev)
        {
            return (dev.Name == null || dev.Name == "" || dev.Id == null);
        }
        static public bool IsSiamSensor(ScannedDeviceInfo dev)
        {
            return dev.Name.Contains("DDIN")
                   || dev.Name.Contains("DDIM")
                   || dev.Name.Contains("SIDDOSA3M")
                   || dev.Name.Contains("DU");
        }
        static bool TryMacToGuid(string mac, out Guid giud)
        {
            string mac_no_delim = mac.ToUpper();
            int exist = mac_no_delim.IndexOf(':');
            //"00000000-0000-0000-0000-0016a4720012"
            while (0 < exist)
            {
                mac_no_delim = mac_no_delim.Remove(exist, 1);
                exist = mac_no_delim.IndexOf(':');
            }
            mac_no_delim = "00000000-0000-0000-0000-" + mac_no_delim;
            return  Guid.TryParse(mac_no_delim, out giud);
        }
        static Guid MacToGuid(string mac)
        {
            if (TryMacToGuid(mac, out Guid guid))
                return guid;
            return new Guid();
        }
    }
}