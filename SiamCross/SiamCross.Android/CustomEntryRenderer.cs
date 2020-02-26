using Android.Content;
using Android.Text.Method;
using SiamCross.Droid;
using SiamCross.Views.ValidationBehavior;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
namespace SiamCross.Droid
{
    public class CustomEntryRenderer : EntryRenderer
    {
        public CustomEntryRenderer(Context ctx) : base(ctx)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            //if (e.OldElement != null || this.Element == null)
            //    return;

            //var CustomEntry = Element as CustomEntry;

            ////  this.Control.KeyListener = Android.Text.Method.DigitsKeyListener.GetInstance(Resources.Configuration.Locale, true, true);
            //this.Control.KeyListener = Android.Text.Method.DigitsKeyListener.GetInstance(string.Format("1234567890{0}", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            //this.Control.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;

            if (Control != null)
            {
                this.Control.KeyListener = DigitsKeyListener.GetInstance(true, true); // I know this is deprecated, but haven't had time to test the code without this line, I assume it will work without
                this.Control.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
            }
        }


        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    if (disposing)
        //    {
        //        if (this != null)
        //        {
        //            Dispose();
        //            this = null;
        //        }
        //    }
        //}
    }
}