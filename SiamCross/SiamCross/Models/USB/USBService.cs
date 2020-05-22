using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using SiamCross.AppObjects;
using SiamCross.Droid.Models;

namespace SiamCross.Models.USB
{
    public class USBService
    {
        private ISerialUsbManager _serialUsbManager;
        #region Singletone implementation

        private static readonly Lazy<USBService> _instance =
            new Lazy<USBService>(() => new USBService());
        public static USBService Instance { get => _instance.Value; }

        #endregion

        private bool _isUsbConnected = false;
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
                    _serialUsbManager.Disconnect();
                }
            }
        }
    
        private Dictionary<string, int> _hardwareDevices;

        public USBService()
        {
            _serialUsbManager = AppContainer.Container.Resolve<ISerialUsbManager>();

            _serialUsbManager.DataReceived += _serialUsbManager_DataReceived;
            _serialUsbManager.ErrorReceived += _serialUsbManager_ErrorReceived;

            System.Diagnostics.Debug.WriteLine("Usb Service Creates!");
        }

        private async Task Initialize()
        {
            await _serialUsbManager.Initialize();
        }

        private void Test()
        {
            new Thread(async () =>
            {
                await Task.Delay(1000);
                await _serialUsbManager.Initialize();
                await Task.Delay(1000);
                _serialUsbManager.TestWrite();
                await Task.Delay(1000);
                _serialUsbManager.Search();
                await Task.Delay(20000);
                _serialUsbManager.TestAddSensor();
            }).Start();
        }

        private void _serialUsbManager_ErrorReceived()
        {
            System.Diagnostics.Debug.WriteLine("Usb Service Error Recieved");
        }

        private void _serialUsbManager_DataReceived()
        {
            System.Diagnostics.Debug.WriteLine("Usb Service Data Recieved");
        }
    }
}
