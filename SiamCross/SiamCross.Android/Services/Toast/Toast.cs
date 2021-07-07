using Android.Widget;
using SiamCross.Services.Toast;
using Xamarin.Essentials;

//[assembly: Dependency(typeof(ToastAndroid))]
namespace SiamCross.Droid.Services.Toast
{
    internal class Toast : IToast
    {
        public void Show(string message, ToastLength length)
        {
            if (MainThread.IsMainThread)
            {
                Android.Widget.Toast.MakeText(
                    Android.App.Application.Context, message, length).Show();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(
                    () => Android.Widget.Toast.MakeText(
                    Android.App.Application.Context, message, length).Show());
            }
        }
        public void LongAlert(string message)
        {
            Show(message, ToastLength.Long);
        }
        public void ShortAlert(string message)
        {
            Show(message, ToastLength.Short);
        }
    }
}