﻿using System;

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

[assembly: UsesFeature("android.hardware.usb.host")]
[assembly: UsesFeature("android.hardware.usb.accessory")]

namespace SiamCross.Droid
{
    [Activity(Label = "SIAM SERVICE 2.0", Icon = "@mipmap/main_icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new[] { UsbManager.ActionUsbDeviceAttached })]
    [MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

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
            }

            // Set it in the constructor
            CurrentActivity = this;

            DetachedReceiver = new UsbDeviceDetachedReceiver();
            RegisterReceiver(DetachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
            AttachedReceiver = new UsbDeviceAttachedReceiver();
            RegisterReceiver(DetachedReceiver, new IntentFilter(UsbManager.ActionUsbDeviceAttached));

            LoadApplication(new App(new Setup()));

        }

        public static Activity CurrentActivity;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #region UsbDeviceDetachedReceiver implementation

        public class UsbDeviceDetachedReceiver
            : BroadcastReceiver
        {
            readonly string TAG = typeof(UsbDeviceDetachedReceiver).Name;
            readonly MainActivity activity;

            public UsbDeviceDetachedReceiver()
            {
                //this.activity = activity;
            }

            public async override void OnReceive(Context context, Intent intent)
            {
                var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

                //Log.Info(TAG, "USB device detached: " + device.DeviceName);

                //await activity.PopulateListAsync();
            }
        }

        #endregion

        #region UsbDeviceAttachedReceiver implementation

        public class UsbDeviceAttachedReceiver
            : BroadcastReceiver
        {
            readonly string TAG = typeof(UsbDeviceAttachedReceiver).Name;
            readonly MainActivity activity;

            public UsbDeviceAttachedReceiver()
            {
                //this.activity = activity;
            }

            public override async void OnReceive(Context context, Intent intent)
            {
                //var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;
                Device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;
                //Log.Info(TAG, "USB device attached: " + device.DeviceName);

                //await activity.PopulateListAsync();
            }
        }

        #endregion

        public static UsbDevice Device { get; set; }
        public UsbDeviceDetachedReceiver DetachedReceiver { get; set; }
        public UsbDeviceAttachedReceiver AttachedReceiver { get; set; }
    }
}