using Android.Runtime;
using Android.Widget;
using SiamCross.Droid.Services;
using SiamCross.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(ToastAndroid))]
namespace SiamCross.Droid.Services
{
    [Preserve(AllMembers = true)]
    public class ToastAndroid : IToast
    {
        public void Show(string message)
        {
            Toast.MakeText(
                Android.App.Application.Context, message, ToastLength.Long).Show();
        }
    }
}