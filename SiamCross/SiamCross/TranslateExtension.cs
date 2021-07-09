using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        private const string ResourceId = "SiamCross.Resource";

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";

            ResourceManager resmgr = new ResourceManager(ResourceId,
                        typeof(TranslateExtension).GetTypeInfo().Assembly);

            string translation = resmgr.GetString(Text, TranslateCfg.ci);

            if (translation == null)
            {
                translation = Text;
            }
            return translation;
        }
    }
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
        public static readonly CultureInfo ci;

        static TranslateCfg()
        {
            string lang2 = Preferences.Get("LanguageKey", "Auto");
            if ("Auto" == lang2)
                ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            else
                ci = new CultureInfo(lang2);

            Thread.CurrentThread.CurrentUICulture = ci;
            Resource.Culture = ci;
        }
        internal static void SetCulture(string lang)
        {
            Preferences.Set("LanguageKey", lang);
        }
    }
}