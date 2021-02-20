using Android.Bluetooth;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Scanners;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SiamCross.Droid.Models
{
    internal class ScannerBt2 : IScannerBt2
    {
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

        public event Action<ScannedDeviceInfo> Received;
        public event Action ScanStoped;
        public event Action ScanStarted;

        public bool IsFilterEnabled { get; set; }

        private readonly IPhyInterface _Phy;
        public IPhyInterface Phy => _Phy;
        public ScannerBt2(IPhyInterface phy)
            : this()
        {
            _Phy = phy;
        }
        public ScannerBt2()
        {
            IsFilterEnabled = true;
        }

        public void Start()
        {
            ScanStarted?.Invoke();
            ICollection<BluetoothDevice> devices = BluetoothAdapter.DefaultAdapter.BondedDevices;

            foreach (BluetoothDevice device in devices)
            {
                if(BluetoothDeviceType.Classic!= device.Type)
                    continue;
                ScannedDeviceInfo sd = new ScannedDeviceInfo
                {
                    Name = device.Name,
                    Mac = device.Address,
                    Id = MacToGuid.Convert(device.Address),
                    BluetoothType = BluetoothType.Classic,
                    BondState = device.BondState.ToString()
                };
                DoNotifyDevice(sd);
            }
            Stop();
        }

        public void Stop()
        {
            ActiveScan = false;
            ScanStoped?.Invoke();
        }

        private void DoNotifyDevice(ScannedDeviceInfo sd)
        {
            if (IsFilterEnabled)
            {

                if (!IsSiamSensor(sd))
                    return;
            }
            Received?.Invoke(sd);
        }
        public static bool IsEmptyDevice(ScannedDeviceInfo dev)
        {
            return (dev.Name == null || dev.Name == "" || dev.Id == null);
        }
        public static bool IsSiamSensor(ScannedDeviceInfo dev)
        {
            if (dev.HasSiamServiceUid || dev.HasUriTag)
                return true;

            if (IsEmptyDevice(dev))
                return false;

            string name = dev.Name.ToUpper();

            return name.Contains("DDIN")
                   || name.Contains("DDIM")
                   || name.Contains("SIDDOSA3M")
                   || name.Contains("DU")
                   || name.Contains("DUA")
                   || name.Contains("UMT")
                   || name.Contains("DMTA")
                   || name.Contains("SIAM")
                   ;
            ;
        }
    }
}