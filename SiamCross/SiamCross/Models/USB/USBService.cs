using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Droid.Models;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;
using Xamarin.Forms;

namespace SiamCross.Models.USB
{
    public class USBService
    {
        private ISerialUsbManager _serialUsbManager;
        #region Singletone implementation

        private static readonly Lazy<USBService> _instance =
            new Lazy<USBService>(() => new USBService());
        public static USBService Instance => _instance.Value;

        #endregion

        private bool _isUsbConnected = false;
        private UsbMessageParcer _usbMessageParcer;
        private bool _isScannig;
        public bool IsUsbConnected
        {
            get => _isUsbConnected;
            set
            {
                _isUsbConnected = value;
                string message = _isUsbConnected ? "UsbAttached" : "UsbDetached";

                MessagingCenter.Send<USBService>(this, message);
            }
        }

        private IUsbDataSubject _dataSubject;

        private HardwareDevicesTable _hardwareDevicesTable;

        public USBService()
        {
            _serialUsbManager = AppContainer.Container.Resolve<ISerialUsbManager>();

            _serialUsbManager.DataReceived += OnUsbDataReceived;
            _serialUsbManager.ErrorReceived += OnUsbErrorReceived;

            _hardwareDevicesTable = new HardwareDevicesTable();
            _dataSubject = new UsbDataDataSubject();
            _usbMessageParcer = new UsbMessageParcer();

            _usbMessageParcer.TableComponentRecieved += OnTableComponentRecieved;
            _usbMessageParcer.DataRecieved += OnDataRecieved;
            _usbMessageParcer.DeviceConnected += OnDeviceConnected;
            _usbMessageParcer.DeviceDisconnected += OnDeviceDisconnected;
            _usbMessageParcer.DeviceFounded += OnDeviceFounded;
            _usbMessageParcer.ScanStarted += OnScanStarted;
            _usbMessageParcer.ScanStopped += OnScanStopped;

            _isScannig = false;

            System.Diagnostics.Debug.WriteLine("Usb Service Creates!");
        }

        public async Task Initialize()
        {
            var isConnect = await _serialUsbManager.Initialize();
            if (isConnect)
            {
                _isUsbConnected = true;
                await _serialUsbManager.Write("10*1*4*");
                await StartScanQuery();
            }
        }

        private void OnUsbErrorReceived()
        {
            System.Diagnostics.Debug.WriteLine("Usb Service Error Received");
        }

        private void OnUsbDataReceived(string stringMessage)
        {
            System.Diagnostics.Debug.WriteLine("Usb Service Data Received");
            try
            {
                _usbMessageParcer.Parse(stringMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void RegisterUsbObserver(IUsbDataObserver observer)
        {
            _dataSubject.Regisеter(observer);
        }

        public void AnregisterUsbObserver(IUsbDataObserver observer)
        {
            _dataSubject.Anregisеter(observer);
        }

        public async Task StartScanQuery()
        {
            if (!IsUsbConnected)
            {
                return;
            }

            // if (!_isScannig)
            // {
            await _serialUsbManager.Write("2*");
            await _serialUsbManager.Write("1*");
           //
           // }
        }

        public async Task ConnectQuery(string address)
        {
            if (!IsUsbConnected)
            {
                return;
            }

            var numberInTable = _hardwareDevicesTable.GetNumberForAddress(address);

            if (numberInTable == -1)
            {
                throw new NotSupportedException(
                    "The device is not in the local modem device table!");
            }

            await _serialUsbManager.Write($"3*{numberInTable}*");
        }

        public async Task TableQuery()
        {
            if (!IsUsbConnected)
            {
                return;
            }

            await _serialUsbManager.Write("*2");
        }

        public async Task SendDataQuery(byte[] data, string senderAddress)
        {
            if (!IsUsbConnected)
            {
                OnDeviceDisconnected(
                    _hardwareDevicesTable.GetNumberForAddress(senderAddress));
                return;
            }

            var strBytes = BitConverter.ToString(data).Replace("-", String.Empty);
            var numberInTable = _hardwareDevicesTable.GetNumberForAddress(senderAddress);

            if (numberInTable == -1)
            {
                throw new NotSupportedException(
                    "The device is not in the local modem device table!");
            }

            await _serialUsbManager.Write($"7*{numberInTable}*{strBytes}");
        }

        public async Task DisconnecDeviceQuery(string senderAddress)
        {
            if (!IsUsbConnected)
            {
                return;
            }

            var numberInTable = _hardwareDevicesTable.GetNumberForAddress(senderAddress);

            if (numberInTable == -1)
            {
                throw new NotSupportedException(
                    "The device is not in the local modem device table!");
            }

            await _serialUsbManager.Write($"*9*{numberInTable}*");
        }
        #region Parcer events handlers

        private void OnDeviceFounded(ScannedDeviceInfo deviceInfo)
        {
            DeviceFounded?.Invoke(deviceInfo);
        }

        private void OnTableComponentRecieved(int numberInTable, string address, string name)
        {
            _hardwareDevicesTable.AddOrRefresh(numberInTable, address);
        }

        private void OnDataRecieved(int numberInTable, byte[] data)
        {
            var observer = GetDataObserver(numberInTable);

            observer?.OnDataRecieved(data);
        }

        private void OnDeviceConnected(int numberInTable)
        {
            var observer = GetDataObserver(numberInTable);

            observer?.OnConnectSucceed();
        }

        private void OnDeviceDisconnected(int numberInTable)
        {
            var observer = GetDataObserver(numberInTable);

            observer?.OnDisconnected();
        }

        private void OnScanStarted()
        {
            _isScannig = true;
        }

        private void OnScanStopped()
        {
            _isScannig = false;
        }

        private IUsbDataObserver GetDataObserver(int numberInTable)
        {
            var address = _hardwareDevicesTable.GetAddressForNumber(numberInTable);
            if (address == null)
            {
                return null;
            }

            var observer = _dataSubject.GetObserverByAddress(address);
            if (observer == null)
            {
                return null;
            }

            return observer;
        }
        #endregion


        public async void OnUsbAttached()
        {
            await Initialize();
            _isUsbConnected = true;
        }

        public void OnUsbDetached()
        {
            var devicesAddresses = _hardwareDevicesTable.GetAllAddresses();
            foreach(var address in devicesAddresses)
            {
                _dataSubject.GetObserverByAddress(address).OnDisconnected();
            }

            _hardwareDevicesTable.Clear();
            _isUsbConnected = false;
        }

        public event Action<ScannedDeviceInfo> DeviceFounded;
    }
}