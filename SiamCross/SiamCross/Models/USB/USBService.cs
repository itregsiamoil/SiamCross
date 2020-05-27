﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.Droid.Models;
using SiamCross.Models.Scanners;

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
                if (_isUsbConnected)
                {
                    new Thread(async () => { await Initialize(); }).Start();
                }
                else
                {
                    _hardwareDevicesTable.Clear();
                    _serialUsbManager.Disconnect();
                }
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

        private async Task Initialize()
        {
            await _serialUsbManager.Initialize();
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

        public void StartScanQuery()
        {
            if (!IsUsbConnected)
            {
                return;
            }

            if (!_isScannig)
            {
                _serialUsbManager.Write("1*");
            }
        }

        public void ConnectQuery(string address)
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

            _serialUsbManager.Write($"*3*{numberInTable}*");
        }

        public void TableQuery()
        {
            if (!IsUsbConnected)
            {
                return;
            }

            _serialUsbManager.Write("*2");
        }

        public void SendDataQuery(byte[] data, string senderAddress)
        {
            if (!IsUsbConnected)
            {
                return;
            }

            string base64data = Convert.ToBase64String(data);
            var numberInTable = _hardwareDevicesTable.GetNumberForAddress(senderAddress);

            if (numberInTable == -1)
            {
                throw new NotSupportedException(
                    "The device is not in the local modem device table!");
            }
        }

        public void DisconnecDeviceQuery(string senderAddress)
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

            _serialUsbManager.Write($"*9*{numberInTable}*");
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

        public event Action<ScannedDeviceInfo> DeviceFounded;
    }
}