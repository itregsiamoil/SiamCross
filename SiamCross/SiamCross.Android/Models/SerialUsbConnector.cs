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


using Android.Util;
using Android.Views;
using Android.Widget;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using System.Globalization;
using System.Threading;

namespace SiamCross.Droid.Models
{
    public class SerialUsbConnector
    {
        const string ACTION_USB_PERMISSION = "USB_PERMISSION";
        private IUsbSerialPort _port;
        private UsbManager _usbManager;
        private SerialIOManager _serialIoManager;
        public SerialUsbConnector()
        {
            _usbManager = Android.App.Application.Context.GetSystemService(Context.UsbService) as UsbManager;

            
            new Thread(async () =>
            {
                var drivers = await FindAllDriversAsync(_usbManager);

                if (drivers.Count == 0) return;

                var driver = drivers.ToArray()[0];
                if (driver == null)
                    throw new Exception("Driver specified in extra tag not found.");
                _port = driver.Ports[0];
                //_port = driver.Ports[portNumber];

                if (_port == null)
                {
                    return;
                }

                GetPremission(driver.Device);

                var portInfo = new UsbSerialPortInfo(_port);
                int vendorId = portInfo.VendorId;
                int deviceId = portInfo.DeviceId;
                int portNumber = portInfo.PortNumber;

                _serialIoManager = new SerialIOManager(_port)
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                };

                
                _serialIoManager.DataReceived += (sender, e) => {
                    //RunOnUiThread(() => {
                    //    UpdateReceivedData(e.Data);
                    //});
                    System.Diagnostics.Debug.WriteLine(e.Data);
                };

                _serialIoManager.ErrorReceived += (sender, e) => {
                    //RunOnUiThread(() => {
                    //    var intent = new Intent(this, typeof(DeviceListActivity));
                    //    StartActivity(intent);
                    //});
                    ;
                };

                try
                {
                    _serialIoManager.Open(_usbManager);
                }
                catch (Java.IO.IOException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    return;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    _serialIoManager.Write("10*5*1*", 50);
                    _serialIoManager.Write("0*", 50);
                    _serialIoManager.Write("0*", 50);
                });
            }).Start();
        }

        internal static Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
        {
            // using the default probe table
            // return UsbSerialProber.DefaultProber.FindAllDriversAsync (usbManager);

            // adding a custom driver to the default probe table
            var table = UsbSerialProber.DefaultProbeTable;
            table.AddProduct(0x1b4f, 0x0008, Java.Lang.Class.FromType(typeof(CdcAcmSerialDriver))); // IOIO OTG
            var prober = new UsbSerialProber(table);
            return prober.FindAllDriversAsync(usbManager);
        }
        
        private async void GetPremission(UsbDevice device)
        {
            if (!_usbManager.HasPermission(device))
            {
                try
                {
                    PendingIntent pi = PendingIntent.GetBroadcast(Forms.Context, 0, new Intent(ACTION_USB_PERMISSION), 0);
                    _usbManager.RequestPermission(device, pi);
                    throw new Exception();
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.Message);
                    //GetPremission(device);
                }
            }
        }     
    }

}