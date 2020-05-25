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

        private IUsbDataSubject _dataSubject;

        public USBService()
        {
            _serialUsbManager = AppContainer.Container.Resolve<ISerialUsbManager>();

            _serialUsbManager.DataReceived += _serialUsbManager_DataReceived;
            _serialUsbManager.ErrorReceived += _serialUsbManager_ErrorReceived;

            _dataSubject = new UsbDataDataSubject();

            System.Diagnostics.Debug.WriteLine("Usb Service Creates!");
        }

        private async Task Initialize()
        {
            await _serialUsbManager.Initialize();
        }

        private void _serialUsbManager_ErrorReceived()
        {
            System.Diagnostics.Debug.WriteLine("Usb Service Error Received");
        }

        private void _serialUsbManager_DataReceived()
        {
            System.Diagnostics.Debug.WriteLine("Usb Service Data Received");
        }
    }
}
