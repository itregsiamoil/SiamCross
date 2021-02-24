using Android.Bluetooth;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using BLE = Android.Bluetooth.LE;

[assembly: Dependency(typeof(IScannerLe))]
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
    public class ScannerLe : IScannerLe
    {
        private readonly BluetoothScanReceiver _receiver = new BluetoothScanReceiver();
        //private static readonly BluetoothAdapter _bt_adapter = BluetoothAdapter.DefaultAdapter;
        private BLE.BluetoothLeScanner _ble_scanner = null;



        public event PropertyChangedEventHandler PropertyChanged;

        private bool _ActiveScan = false;
        public bool ActiveScan
        {
            get => _ActiveScan;
            private set
            {
                _ActiveScan = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveScan)));
            }
        }

        private string _ScanString;
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

        private readonly IPhyInterface _Phy;
        public IPhyInterface Phy => _Phy;

        public event Action<ScannedDeviceInfo> Received;
        public event Action ScanStoped;
        public event Action ScanStarted;

        public ScannerLe(IPhyInterface phy)
            : this()
        {
            _Phy = phy;
        }
        public ScannerLe()
        {
            ScanTimeout = 10000;
            IsFilterEnabled = true;

            _receiver.ActionOnScanResult += OnScanResult;
        }

        public void OnScanResult(BLE.ScanResult result)
        {
            if (BLE.DataStatus.Complete != result.DataStatus)
                return;

            ScannedDeviceInfo sd = new ScannedDeviceInfo
            {
                BluetoothType = BluetoothType.Le,
                Name = result.Device.Name,
                Mac = result.Device.Address,
                Id = MacToGuid.Convert(result.Device.Address),
                BondState = result.Device.BondState.ToString()
            };
            sd.ProtocolData["PrimaryPhy"] = ((BluetoothPhy)result.PrimaryPhy).ToString();
            sd.ProtocolData["SecondaryPhy"] = ((BluetoothPhy)result.SecondaryPhy).ToString();
            sd.ProtocolData["Rssi"] = result.Rssi.ToString();
            sd.ProtocolData["IsLegacy"] = result.IsLegacy.ToString();
            sd.ProtocolData["IsConnectable"] = result.IsConnectable.ToString();
            sd.ProtocolData["TxPower"] = (BLE.ScanResult.TxPowerNotPresent == (int)result.TxPower) ?
                    "TxPowerNotPresent" : ((BLE.AdvertiseTxPower)result.TxPower).ToString();

            bool HasSiamServiceUid = false;
            bool HasUriTag = false;

            IList<Android.OS.ParcelUuid> svc = result.ScanRecord.ServiceUuids;
            if (null != svc)
            {
                //List<string> list = new List<string>();
                foreach (Android.OS.ParcelUuid s in svc)
                {
                    if (s.ToString() == "569a1101-b87f-490c-92cb-11ba5ea5167c")
                    {
                        HasSiamServiceUid = true;
                        DoNotifyDevice(sd);
                        return;
                    }
                }
            }
            /*
            Java.Util.UUID uuid1101 = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
            Android.OS.ParcelUuid fpu1101 = new Android.OS.ParcelUuid(uuid1101);
            byte[] uuid_data = result.ScanRecord.GetServiceData(fpu1101);
            if(null!= uuid_data)
            {
                string bytesUtf8 = Encoding.UTF8.GetString(uuid_data).ToUpper();
                if (bytesUtf8 .Contains("СИАМ") || bytesUtf8 .Contains("SIAM"))
                    sd.HasUriTag = true;
            }
            List<object> scan_info = new List<object>();
            */
            byte[] bt = result.ScanRecord.GetBytes();
            int pos = 0;
            while (pos + 2 < bt.GetLength(0))
            {
                byte data_block_len = bt[pos];
                data_block_len--;

                pos++;
                if (pos > bt.GetLength(0))
                    break;
                byte data_block_type = bt[pos];
                pos++;
                if (pos > bt.GetLength(0))
                    break;

                if (0x24 == data_block_type || 0x16 == data_block_type)
                {
                    byte[] data_block_data = bt.AsSpan(pos, data_block_len).ToArray();
                    string bytesUtf8 = Encoding.UTF8.GetString(data_block_data).ToUpper();
                    if (bytesUtf8.Contains("СИАМ") || bytesUtf8.Contains("SIAM"))
                    {
                        HasUriTag = true;
                        break;
                    }
                }
                //var v = new { Len = data_block_len , Type = data_block_type, Data = data_block_data };
                //scan_info.Add(v);
                pos += data_block_len;
            }

            if (IsFilterEnabled)
            {
                if (sd.Id != null && (HasSiamServiceUid || HasUriTag || IsSiamSensor(sd.Name)))
                    DoNotifyDevice(sd);
            }
            else
                DoNotifyDevice(sd);
        }

        public void AddBonded()
        {
            ScanStarted?.Invoke();
            ICollection<BluetoothDevice> devices = BluetoothAdapter.DefaultAdapter.BondedDevices;

            foreach (BluetoothDevice device in devices)
            {
                if (BluetoothDeviceType.Le != device.Type)
                    continue;
                ScannedDeviceInfo sd = new ScannedDeviceInfo
                {
                    Name = device.Name,
                    Mac = device.Address,
                    Id = MacToGuid.Convert(device.Address),
                    BluetoothType = BluetoothType.Le,
                };
                sd.ProtocolData["BondState"] = device.BondState.ToString();

                if (IsFilterEnabled)
                {
                    if (sd.Id != null && IsSiamSensor(sd.Name))
                        DoNotifyDevice(sd);
                }
                else
                    DoNotifyDevice(sd);
            }
        }
        public async void Start()
        {
            string info = SiamCross.Resource.SearchTitle + " BLE ... ";
            StartScanInfo(info, ScanTimeout / 1000);
            ScanStarted?.Invoke();
            AddBonded();
            BLE.ScanSettings.Builder b = new BLE.ScanSettings.Builder();
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
            _ble_scanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;
            _ble_scanner.StartScan(null, st, _receiver);
            await Task.Delay(ScanTimeout);
            Stop();
        }

        public void Stop()
        {
            if (null == _ble_scanner)
                return;
            _ble_scanner.StopScan(_receiver);
            _ble_scanner.FlushPendingScanResults(_receiver);

            ClearScanInfo();
            _ble_scanner?.Dispose();
            _ble_scanner = null;
            ScanStoped?.Invoke();
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
            Received?.Invoke(sd);
        }
        public static bool IsSiamSensor(string dvc_name)
        {
            if (dvc_name == null)
                return false;
            string name = dvc_name.ToUpper();
            return name.Contains("DDIN")
                   || name.Contains("DDIM")
                   || name.Contains("SIDDOSA3M")
                   || name.Contains("DU")
                   || name.Contains("DUA")
                   || name.Contains("UMT")
                   || name.Contains("DMTA")
                   || name.Contains("SIAM")
                   ;
        }


    }
}