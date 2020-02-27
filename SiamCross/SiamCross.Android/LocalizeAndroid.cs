using Xamarin.Forms;
[assembly: Dependency(typeof(LocalizeApp.Droid.Localize))]

namespace LocalizeApp.Droid
{
    public class Localize : ILocalize
    {
        public System.Globalization.CultureInfo GetCurrentCultureInfo()
        {
            var androidLocale = Java.Util.Locale.Default;
            var netLanguage = androidLocale.ToString().Replace("_", "-");
            try
            {
                return new System.Globalization.CultureInfo(netLanguage);
            }
            catch
            {
                return new System.Globalization.CultureInfo("en-US");
            }
        }
    }
}