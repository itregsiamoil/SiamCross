using Android.Widget;
using SiamCross.Services.Toast;

//[assembly: Dependency(typeof(ToastAndroid))]
namespace SiamCross.Droid.Services.Toast
{
    internal class Toast : IToast
    {
        public void LongAlert(string message)
        {
            Android.Widget.Toast.MakeText(
                Android.App.Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Android.Widget.Toast.MakeText(
                Android.App.Application.Context, message, ToastLength.Short).Show();

        }
    }
}