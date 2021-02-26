using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Models;
using SiamCross.Models.Adapters;
using SiamCross.Models.Adapters.PhyInterface;
using SiamCross.Models.Connection.Phy;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Connection.Protocol.Siam;
using SiamCross.Models.Scanners;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        //public string Title => Scanner.Phy.Name;

        public ObservableCollection<ScannedDeviceInfo> ScannedDevices { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool _Detecting = false;
        public bool Detecting => _Detecting;

        public readonly MemStruct _Common = new MemStruct(0x00);
        public readonly MemVarUInt16 DeviceType;
        public readonly MemVarUInt16 MemoryModelVersion;
        public readonly MemVarUInt32 DeviceNameAddress;
        public readonly MemVarUInt16 DeviceNameSize;
        public readonly MemVarUInt32 DeviceNumber;
        //public readonly MemStruct _Info;
        //public readonly MemVarUInt32 ProgrammVersionAddress;
        //public readonly MemVarUInt16 ProgrammVersionSize;

        public ICommand RefreshCommand { get; }
        public ICommand SelectItemCommand { get; }
        public ICommand StartStopScanCommand { get; }

        public ScannerViewModel(IBluetoothScanner scanner)
        {
            _scanner = scanner;
            ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();

            _scanner.Received += ScannerReceivedDevice;
            _scanner.ScanStoped += ScannerScanTimoutElapsed;

            RefreshCommand = new Command(StartScan);
            SelectItemCommand = new Command(SelectItem);
            StartStopScanCommand = new Command(StartStopScan);

            DeviceType = _Common.Add(new MemValueUInt16(), nameof(DeviceType));
            MemoryModelVersion = _Common.Add(new MemValueUInt16(), nameof(MemoryModelVersion));
            DeviceNameAddress = _Common.Add(new MemValueUInt32(), nameof(DeviceNameAddress));
            DeviceNameSize = _Common.Add(new MemValueUInt16(), nameof(DeviceNameSize));
            DeviceNumber = _Common.Add(new MemValueUInt32(), nameof(DeviceNumber));

            //_Info = new MemStruct(0x1000);
            //ProgrammVersionAddress = _Info.Add(new MemVarUInt32(), nameof(ProgrammVersionAddress));
            //ProgrammVersionSize = _Info.Add(new MemVarUInt16(), nameof(ProgrammVersionSize));

            StartScan();
        }

        private void ScannerScanTimoutElapsed()
        {
        }

        private void ScannerReceivedDevice(ScannedDeviceInfo dev)
        {
            if (ScannedDevices.Contains(dev))
                return;
            ScannedDevices.Add(dev);
        }

        private async void SelectItem(object obj)
        {
            if (obj is ScannedDeviceInfo dev)
                await SelectItemAsync(dev);
        }
        public async Task SelectItemAsync(ScannedDeviceInfo item)
        {
            try
            {
                if (item == null)
                    return;

                Dictionary<string, SiamDeviceInfo> devices = await GetDevice(item);
                if (null == devices || 0 == devices.Count)
                    return;
                string action = await Application.Current.MainPage
                    .DisplayActionSheet("Select device"
                    , "Cancel", null, devices.Keys.AsEnumerable().ToArray());
                if (action == "Cancel")
                    return;
                if (!devices.TryGetValue(action, out SiamDeviceInfo siam_device))
                    return;

                item.Name = $"{siam_device.Name} №{siam_device.Num}";
                item.Name = item.Name.Replace("\0", string.Empty);
                item.Name = item.Name.Replace("\r", string.Empty);
                item.Name = item.Name.Replace("\n", string.Empty);
                item.Name = item.Name.Replace("\t", string.Empty);
                item.ProtocolAddress = siam_device.Address;
                item.Kind = siam_device.Kind;

                SensorService.Instance.AddSensor(item);
                await App.NavigationPage.Navigation.PopToRootAsync();
                App.MenuIsPresented = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ItemSelected (creating sensor)" + "\n");
            }
        }

        private async Task<Dictionary<string, SiamDeviceInfo>> GetDevice(ScannedDeviceInfo phy_item)
        {
            IPhyConnection phy_conn = null;
            Dictionary<string, SiamDeviceInfo> dir = new Dictionary<string, SiamDeviceInfo>();
            try
            {
                _Detecting = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Detecting)));
                
                IPhyInterface phy_interface = null;
                switch (phy_item.BluetoothType)
                {
                    default: break;
                    case BluetoothType.Le:
                        phy_interface = FactoryBtLe.GetCurent(); break;
                    case BluetoothType.Classic:
                        phy_interface = FactoryBt2.GetCurent(); break;
                }
                phy_conn = phy_interface.MakeConnection(phy_item);
                switch (phy_item.ProtocolKind)
                {
                    case 0: // ProtocolKind.Siam:
                        IProtocolConnection connection = new SiamConnection(phy_conn);
                        if (await connection.Connect() && RespResult.NormalPkg == await connection.ReadAsync(_Common))
                        {
                            UInt32 address = DeviceNameAddress.Value;
                            UInt16 len = DeviceNameSize.Value;
                            byte[] membuf = new byte[len];
                            await connection.ReadMemAsync(address, len, membuf);
                            string dvc_name;
                            if (10 > MemoryModelVersion.Value)
                                dvc_name = Encoding.GetEncoding(1251).GetString(membuf, 0, len);
                            else
                                dvc_name = Encoding.UTF8.GetString(membuf, 0, len);

                            //await connection.ReadAsync(_Info);
                            //address = ProgrammVersionAddress.Value;
                            //len = ProgrammVersionSize.Value;
                            //membuf = new byte[len];
                            //await connection.ReadMemAsync(address, len, membuf);
                            //string firmware;
                            //if (10 > MemoryModelVersion.Value)
                            //    firmware = Encoding.GetEncoding(1251).GetString(membuf, 0, len);
                            //else
                            //    firmware = Encoding.UTF8.GetString(membuf, 0, len);

                            string label;
                            label = $"{dvc_name} №{DeviceNumber.Value}"
                                + $"\n{Resource.Address}: { phy_item.ProtocolAddress}"
                                + $" {Resource.Type}: 0x" + DeviceType.Value.ToString("X2");

                            SiamDeviceInfo siam_device = new SiamDeviceInfo(dvc_name
                                , DeviceNumber.Value.ToString(), phy_item.ProtocolAddress, DeviceType.Value);

                            dir.Add(label, siam_device);

                        }
                        await connection.Disconnect();
                        break;
                    default:
                    case 1://ProtocolKind.Modbus: 
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            finally
            {
                _Detecting = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Detecting)));
                await phy_conn.Disconnect();
            }
            return dir;
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
                //ScannedDevices = new ObservableCollection<ScannedDeviceInfo>();
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
