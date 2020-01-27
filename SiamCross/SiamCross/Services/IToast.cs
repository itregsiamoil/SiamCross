using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public interface IToast
    {
        void Show(string message);
    }
}
