using System;
using System.Globalization;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross
{
    [ContentProperty("Source")]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }
            ImageSource imageSource = ImageSource.FromResource(Source);

            return imageSource;
        }
    }
    public static class TranslateCfg
    {
        public static CultureInfo Current
        {
            get
            {
                try
                {
                    string lang = Preferences.Get("LanguageKey", "Auto");
                    return CultureInfo.GetCultureInfo(lang);
                }
                catch (Exception)
                {
                }
                return DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            }
        }
        internal static void SetCulture(string lang)
        {
            if ("Auto" == lang)
            {
                Preferences.Remove("LanguageKey");
                LocalizationResourceManager.Current.CurrentCulture = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            }
            else
            {
                Preferences.Set("LanguageKey", lang);
                LocalizationResourceManager.Current.CurrentCulture = CultureInfo.GetCultureInfo(lang);
            }
        }
        public static string[] SupportedLanguages => new string[]
                {
                    Resource.System,
                    "Русский",
                    "English"
                };
    }
}