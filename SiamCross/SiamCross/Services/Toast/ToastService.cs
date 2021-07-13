using System;

namespace SiamCross.Services.Toast
{
    public sealed class ToastService : IToast
    {
        private static readonly Lazy<ToastService> _instance =
            new Lazy<ToastService>(() => new ToastService());
        public static IToast Instance => _instance.Value;

        private readonly IToast _object;
        private ToastService()
        {
            _object = Xamarin.Forms.DependencyService.Get<IToast>();
        }
        public void LongAlert(string message)
        {
            _object.LongAlert(message);
        }
        public void ShortAlert(string message)
        {
            _object.ShortAlert(message);
        }
        public static void Show(string message, bool shortDelay = false)
        {
            if (shortDelay)
                Instance.LongAlert(message);
            else
                Instance.ShortAlert(message);
        }
    }
}
