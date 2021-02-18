using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Connection.Protocol.Siam;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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


        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; private set; }

        public ObservableCollection<ScannedDeviceInfo> ClassicDevices { get; private set; }

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

            _Common = new MemStruct(0x00);
            DeviceType = _Common.Add(new MemVarUInt16(), nameof(DeviceType));
            MemoryModelVersion = _Common.Add(new MemVarUInt16(), nameof(MemoryModelVersion));
            DeviceNameAddress = _Common.Add(new MemVarUInt32(), nameof(DeviceNameAddress));
            DeviceNameSize = _Common.Add(new MemVarUInt16(), nameof(DeviceNameSize));
            DeviceNumber = _Common.Add(new MemVarUInt32(), nameof(DeviceNumber));

            _Info = new MemStruct(0x1000);
            ProgrammVersionAddress = _Info.Add(new MemVarUInt32(), nameof(ProgrammVersionAddress));
            ProgrammVersionSize = _Info.Add(new MemVarUInt16(), nameof(ProgrammVersionSize));

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
                SelectItemAsync(dev);
        }
        public async Task SelectItemAsync(ScannedDeviceInfo item)
        {
            try
            {
                if (item == null)
                    return;

                var devices = await GetDevice(item);
                

                string action = await Application.Current.MainPage
                    .DisplayActionSheet("Select device"
                    , "Cancel", null, devices.Keys.AsEnumerable().ToArray() );
                if (action == "Cancel")
                    return;


                SensorService.Instance.AddSensor(item);
                await App.NavigationPage.Navigation.PopToRootAsync();
                App.MenuIsPresented = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ItemSelected (creating sensor)" + "\n");
            }
        }
        private async Task< Dictionary<string, int> > GetDevice(ScannedDeviceInfo phy_item)
        {
            Dictionary<string, int> dir = new Dictionary<string, int>();
            IPhyInterface phy_interface = null;
            switch (phy_item.BluetoothType)
            {
                default: break;
                case BluetoothType.Le:
                    phy_interface = BtLeInterface.Factory.GetCurent(); break;
                case BluetoothType.Classic:
                    phy_interface = Models.Adapters.PhyInterface.Bt2.Factory.GetCurent(); break;
            }
            IPhyConnection conn = phy_interface.MakeConnection(phy_item);

            switch (phy_item.Protocol.Kind)
            {
                case ProtocolKind.Siam:
                    IProtocolConnection connection = new SiamConnection(conn);
                    await connection.Connect();
                    await connection.ReadAsync(_Common);
                    await connection.ReadAsync(_Info);

                    UInt32 address = DeviceNameAddress.Value;
                    UInt16 len = DeviceNameSize.Value;
                    byte[] membuf = new byte[len];
                    await connection.ReadMemAsync(address, len, membuf);
                    string dvc_name;
                    if (10 > MemoryModelVersion.Value)
                        dvc_name = Encoding.GetEncoding(1251).GetString(membuf,0,len);
                    else
                        dvc_name = Encoding.UTF8.GetString(membuf, 0, len);

                    address = ProgrammVersionAddress.Value;
                    len = ProgrammVersionSize.Value;
                    membuf = new byte[len];
                    await connection.ReadMemAsync(address, len, membuf);
                    string firmware;
                    if (10 > MemoryModelVersion.Value)
                        firmware = Encoding.GetEncoding(1251).GetString(membuf, 0, len);
                    else
                        firmware = Encoding.UTF8.GetString(membuf, 0, len);


                    string label;
                    label = $"{DeviceType.Value}0 {dvc_name} {DeviceNumber.Value} {firmware}";
                    dir.Add(label, phy_item.Protocol.Address);
                    label = $"{DeviceType.Value}1 {dvc_name} {DeviceNumber.Value} {firmware}";
                    dir.Add(label, phy_item.Protocol.Address);
                    label = $"{DeviceType.Value}2 {dvc_name} {DeviceNumber.Value} {firmware}";
                    dir.Add(label, phy_item.Protocol.Address);
                    break;
                default:
                case ProtocolKind.Modbus: break;
            }

            return dir;
        }


        public readonly MemStruct _Common;
        public readonly MemVarUInt16 DeviceType;
        public readonly MemVarUInt16 MemoryModelVersion;
        public readonly MemVarUInt32 DeviceNameAddress;
        public readonly MemVarUInt16 DeviceNameSize;
        public readonly MemVarUInt32 DeviceNumber;
        public readonly MemStruct _Info;
        public readonly MemVarUInt32 ProgrammVersionAddress;
        public readonly MemVarUInt16 ProgrammVersionSize;
        public readonly MemStruct _SurvayParam;


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
                //ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
                //ClassicDevices = new ObservableCollection<ScannedDeviceInfo>();
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
