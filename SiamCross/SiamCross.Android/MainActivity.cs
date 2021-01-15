using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Xaml;

namespace SiamCross.Droid
{
    [Activity(Label = "SIAM SERVICE", Icon = "@mipmap/main_icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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

            LoadApplication(new App(new Setup()));
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
    }
}