using System;
using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Bluetooth;
using Android.Content;
using Android.Hardware.Usb;
using Hoho.Android.UsbSerial.Extensions;
using Hoho.Android.UsbSerial.Driver;
using SiamCross.Models.USB;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;

[assembly: UsesFeature("android.hardware.usb.host")]
[assembly: UsesFeature("android.hardware.usb.accessory")]

namespace SiamCross.Droid
{
    [Activity(Label = "SIAM SERVICE", Icon = "@mipmap/main_icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            // set the layout resources first
            FormsAppCompatActivity.ToolbarResource = Resource.Layout.Toolbar;
            FormsAppCompatActivity.TabLayoutResource = Resource.Layout.Tabbar;

            base.OnCreate(savedInstanceState);
            //Xamarin.Forms.Forms.SetFlags("Expander_Experimental");
            Xamarin.Forms.Forms.SetFlags("SwipeView_Experimental");
            bool all_granted = false;
            while (!all_granted)
            {
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);
                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                var locationPermissions = new[]
                {
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.WriteExternalStorage
                };

                // check if the app has permission to access coarse location
                var coarseLocationPermissionGranted =
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation);

                // check if the app has permission to access fine location
                var fineLocationPermissionGranted =
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation);

                var externalFilesPermissionGranted =
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage);

                // if either is denied permission, request permission from the user
                const int locationPermissionsRequestCode = 1000;
                if (coarseLocationPermissionGranted == Permission.Denied ||
                    fineLocationPermissionGranted == Permission.Denied ||
                    externalFilesPermissionGranted == Permission.Denied)
                {
                    ActivityCompat.RequestPermissions(this, locationPermissions, locationPermissionsRequestCode);
                    mAllPermOkExecTcs = new TaskCompletionSource<bool>();
                    all_granted = await mAllPermOkExecTcs.Task;
                    await Task.Delay(1000);
                }
                else
                    all_granted = true;
            }

            // Set it in the constructor
            CurrentActivity = this;

            DetachedReceiver = new UsbDeviceDetachedReceiver();
            RegisterReceiver(DetachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
            AttachedReceiver = new UsbDeviceAttachedReceiver();
            RegisterReceiver(DetachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));

            LoadApplication(new App(new Setup()));
            new Thread(async () => { await USBService.Instance.Initialize(); }).Start();
        }

        public static Activity CurrentActivity;

        private TaskCompletionSource<bool> mAllPermOkExecTcs;
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            System.Diagnostics.Debug.WriteLine( $"OnRequestPermissionsResult(requestCode={requestCode} " +
                $"- Permissions Count={permissions.Length} - GrantResults Count={grantResults.Length})");

            bool all_granted = true;
            foreach(var g  in grantResults)
            {
                if(g!= Permission.Granted)
                {
                    all_granted = false;
                    Toast.MakeText(this, "You must approve all permissions", ToastLength.Long).Show();
                    break;
                }
            }
            mAllPermOkExecTcs?.TrySetResult(all_granted);
        }

        #region UsbDeviceDetachedReceiver implementation

        public class UsbDeviceDetachedReceiver
            : BroadcastReceiver
        {
            private readonly string attached = "android.hardware.usb.action.USB_DEVICE_ATTACHED";
            private readonly string detached = "android.hardware.usb.action.USB_DEVICE_DETACHED";

            public override void OnReceive(Context context, Intent intent)
            {
                if (intent.Action == attached)
                {
                    System.Diagnostics.Debug.WriteLine(intent.Action);
                    USBService.Instance.OnUsbAttached();
                }

                if (intent.Action == detached)
                {
                    System.Diagnostics.Debug.WriteLine(intent.Action);
                    USBService.Instance.OnUsbDetached();
                }
            }
        }

        #endregion

        #region UsbDeviceAttachedReceiver implementation

        public class UsbDeviceAttachedReceiver
            : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent) { }
        }

        #endregion

        public static UsbDevice Device { get; set; }
        public UsbDeviceDetachedReceiver DetachedReceiver { get; set; }
        public UsbDeviceAttachedReceiver AttachedReceiver { get; set; }
    }
}