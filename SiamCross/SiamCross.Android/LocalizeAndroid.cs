using Xamarin.Forms;
[assembly: Dependency(typeof(LocalizeApp.Droid.Localize))]

namespace LocalizeApp.Droid
{
    public class Localize : ILocalize
    {
        public System.Globalization.CultureInfo GetCurrentCultureInfo()
        {
            Java.Util.Locale androidLocale = Java.Util.Locale.Default;
            string netLanguage = androidLocale.ToString().Replace("_", "-");
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