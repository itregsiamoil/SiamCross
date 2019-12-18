using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SiamCross.Views
{
    public class FloatNumberValidationBehavior : NumericValidationBehaviorBase
    {
        protected override void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                bool isValid =
                    args.NewTextValue.ToCharArray()
                    .All(x => char.IsDigit(x) ||
                    x == Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                ((Entry)sender).Text =
                    isValid ? args.NewTextValue : args.NewTextValue.Remove(
                        args.NewTextValue.Length - 1);
            }
        }
    }
}
