using System;

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

namespace SiamCross.Droid
{
    [Activity(Label = "SIAM SERVICE 2.0", Icon = "@mipmap/main_icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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

            //BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            //if (bluetoothAdapter.IsEnabled)
            //{
            //    bluetoothAdapter.Disable();
            //    bluetoothAdapter.Enable();
            //}
            //else
            //{
            //    bluetoothAdapter.Enable();
            //}

            // Set it in the constructor
            CurrentActivity = this;

            LoadApplication(new App(new Setup()));
        }

        public static Activity CurrentActivity;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}