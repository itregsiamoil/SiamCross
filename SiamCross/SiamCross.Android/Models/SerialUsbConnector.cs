using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using Xamarin.Forms;

using Hoho.Android.UsbSerial.Extensions;

namespace SiamCross.Droid.Models
{
    public class SerialUsbConnector : ISerialUsbManager
    {
        const string ACTION_USB_PERMISSION = "USB_PERMISSION";
        public const string EXTRA_TAG = "PortInfo";
        private UsbSerialPort _port;
        private UsbManager _usbManager;
        private SerialInputOutputManager _serialIoManager;
        //private UsbDeviceDetachedReceiver _detachedReceiver;
        //private UsbDeviceAttachedReceiver _attachedReceiver;

        public SerialUsbConnector()
        {
            //register the broadcast receivers
            //_detachedReceiver = new UsbDeviceDetachedReceiver();
            //RegisterReceiver(_detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
            //_attachedReceiver = new UsbDeviceAttachedReceiver();
            //RegisterReceiver(_detachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));
        }

        public async Task Test()
        {
        //        var LibSerialPort = new SerialPort(
        //        "/dev/tty",
        //        115200,
        //        Stopbits.One,
        //        Parity.None,
        //        ByteSize.EightBits,
        //        FlowControl.Software,
        //        new Timeout(50, 50, 50, 50, 50));

        //    LibSerialPort.OnReceived += SerialPort_OnReceived;
        }

        public async Task Initialize()
        {
            _usbManager = Android.App.Application.Context.GetSystemService(Context.UsbService) as UsbManager;
            var drivers = await FindAllDriversAsync(_usbManager);

            if (drivers.Count == 0) return;

            var driver = drivers.ToArray()[0];
            if (driver == null)
                throw new Exception("Driver specified in extra tag not found.");
            _port = driver.Ports[0];           

            if (_port == null)
            {
                return;
            }

            GetPremission(driver.Device);

            var portInfo = new UsbSerialPortInfo(_port);

            ////var portInfo = MainActivity.CurrentActivity.Intent.GetParcelableExtra(EXTRA_TAG) as UsbSerialPortInfo;
            int vendorId = portInfo.VendorId;
            int deviceId = portInfo.DeviceId;
            int portNumber = portInfo.PortNumber;

             _port = driver.Ports[portNumber];           

            if (_port == null)
            {
                return;
            }

            _serialIoManager = new SerialInputOutputManager(_port)
            {
                BaudRate = 115200,
                //BaudRate = 19200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
            };


            _serialIoManager.DataReceived += (sender, e) =>
            {
                //RunOnUiThread(() => {
                //    UpdateReceivedData(e.Data);
                //});
                System.Diagnostics.Debug.WriteLine("\nModem: " + 
                                                   Encoding.Default.GetString(e.Data) + "\n");
            };

            _serialIoManager.ErrorReceived += (sender, e) =>
            {
                //RunOnUiThread(() => {
                //    var intent = new Intent(this, typeof(DeviceListActivity));
                //    StartActivity(intent);
                //});
                ;
            };

            try
            {
                _serialIoManager.Open(_usbManager);
                //_port.SetRTS(true);
            }
            catch (Java.IO.IOException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return;
            }
        }

        public void TestWrite()
        {
            WriteData("10*1*4*\r\n");
        }

        public void Search()
        {
            WriteData("1*\r\n");
        }

        public void TestAddSensor()
        {
            WriteData("3*0*\r\n");
        }

        internal static Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
        {
            // using the default probe table
            // return UsbSerialProber.DefaultProber.FindAllDriversAsync (usbManager);

            // adding a custom driver to the default probe table
            //var table = UsbSerialProber.DefaultProbeTable;
            //table.AddProduct(0x1b4f, 0x0008, Java.Lang.Class.FromType(typeof(CdcAcmSerialDriver))); // IOIO OTG
            //var prober = new UsbSerialProber(table);
            //return prober.FindAllDriversAsync(usbManager);

            var table = UsbSerialProber.DefaultProbeTable;
            table.AddProduct(0x1b4f, 0x0008, typeof(CdcAcmSerialDriver)); // IOIO OTG

            table.AddProduct(0x09D8, 0x0420, typeof(CdcAcmSerialDriver)); // Elatec TWN4

            table.AddProduct(0x10C4, 0x8293, typeof(Cp21xxSerialDriver)); 


            var prober = new UsbSerialProber(table);
            return prober.FindAllDriversAsync(usbManager);
        }
        
        private async void GetPremission(UsbDevice device)
        {
            if (!_usbManager.HasPermission(device))
            {
                try
                {
                    var permissionGranted = await _usbManager.RequestPermissionAsync(device, Forms.Context);////
                    //PendingIntent pi = PendingIntent.GetBroadcast(Forms.Context, 0, new Intent(ACTION_USB_PERMISSION), 0);
                    //_usbManager.RequestPermission(device, pi);
                    throw new Exception();
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.Message);
                    //GetPremission(device);
                }
            }
        }

        private void WriteData(string str)
        {
            try
            {
                if (_serialIoManager.IsOpen)
                {
                    List<byte> buff = new List<byte>();
                    buff.AddRange(Encoding.ASCII.GetBytes(str));
                    _port.Write(buff.ToArray(), 200);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Write data error!");
            }
        }

        public void ConnectAndSend()
        {
            throw new NotImplementedException();
        }
    }
}