using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.Android;

namespace SiamCross.Droid
{
    [Activity(Label = "SIAM SERVICE",
        Icon = "@mipmap/main_icon",
        Theme = "@style/Theme.Splash",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        public static Activity CurrentActivity;
        private TaskCompletionSource<bool> mAllPermOkExecTcs;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.MainTheme);
            // set the layout resources first
            FormsAppCompatActivity.ToolbarResource = Resource.Layout.Toolbar;
            FormsAppCompatActivity.TabLayoutResource = Resource.Layout.Tabbar;
            base.OnCreate(savedInstanceState);
            await GetPermissionsAsync();

            //Xamarin.Forms.Forms.SetFlags("Expander_Experimental");
            Xamarin.Forms.Forms.SetFlags("SwipeView_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            CurrentActivity = this;
            // Set it in the constructor
            LoadApplication(new App(new Setup()));
        }
        public async Task GetPermissionsAsync()
        {
            List<string> not_granted_perm = new List<string>();
            string[] all_perm =
            {
                Manifest.Permission.Bluetooth,
                Manifest.Permission.BluetoothAdmin,

                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage,

                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation,

                //Manifest.Permission.SystemAlertWindow,
            };

            do
            {
                not_granted_perm.Clear();
                foreach (string perm in all_perm)
                {
                    // check if the app has permission to access 
                    if (Permission.Granted != ContextCompat.CheckSelfPermission(this, perm))
                        not_granted_perm.Add(perm);
                }
                if (0 != not_granted_perm.Count)
                {
                    const int request_code = 1000;
                    ActivityCompat.RequestPermissions(this, not_granted_perm.ToArray(), request_code);
                    mAllPermOkExecTcs = new TaskCompletionSource<bool>();
                    bool all_granted = await mAllPermOkExecTcs.Task;
                    if (!all_granted)
                    {
                        Toast.MakeText(this, "You must approve all permissions", ToastLength.Long).Show();
                        await Task.Delay(1000);
                    }
                }
            }
            while (0 < not_granted_perm.Count);
        }//public async Task GetPermissionsAsync()

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            System.Diagnostics.Debug.WriteLine($"OnRequestPermissionsResult(requestCode={requestCode} " +
                $"- Permissions Count={permissions.Length} - GrantResults Count={grantResults.Length})");

            bool all_granted = true;
            foreach (Permission g in grantResults)
            {
                if (g != Permission.Granted)
                {
                    all_granted = false;
                    break;
                }
            }
            mAllPermOkExecTcs?.TrySetResult(all_granted);
        }
    }
}