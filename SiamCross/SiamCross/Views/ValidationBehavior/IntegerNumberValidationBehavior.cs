﻿using System.Linq;
using Xamarin.Forms;

namespace SiamCross.Views
{
    public class IntegerNumberValidationBehavior : NumericValidationBehaviorBase
    {
        protected override void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                bool isValid =
                    args.NewTextValue.ToCharArray().All(x => char.IsDigit(x));
                ((Entry)sender).Text =
                    isValid ? args.NewTextValue : args.NewTextValue.Remove(
                        args.NewTextValue.Length - 1);
            }
        }
    }
}
