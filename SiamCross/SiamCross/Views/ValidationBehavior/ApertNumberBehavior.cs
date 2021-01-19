using System;
using System.Linq;
using Xamarin.Forms;

namespace SiamCross.Views
{
    public class ApertNumberBehavior : NumericValidationBehaviorBase
    {
        protected override void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                if (args.OldTextValue != null && args.OldTextValue.Length == 1)
                {
                    ((Entry)sender).Text = args.NewTextValue.Remove(args.NewTextValue.Length - 1);
                }
                else
                {
                    bool isValid =
                        args.NewTextValue.ToCharArray().All(x => char.IsDigit(x));
                    if (isValid)
                    {
                        char num = args.NewTextValue[args.NewTextValue.Length - 1];
                        if (Int32.TryParse(num.ToString(), out int number))
                        {

                        }
                    }
                    ((Entry)sender).Text =
                        isValid ? args.NewTextValue : args.NewTextValue.Remove(
                            args.NewTextValue.Length - 1);
                }

            }
        }
    }
}
