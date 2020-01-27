using Autofac;
using Xamarin.Forms.Internals;

namespace SiamCross.AppObjects
{
    [Preserve(AllMembers = true)]
    public static class AppContainer
    {
        public static IContainer Container { get; set; }
    }
}
