using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace SiamCross.Views.ValidationBehavior
{
    public class CustomEntry : Entry
    {
        public void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (CustomEntry)sender;
            string nonDecimalSeparator = "";
            if (CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator == ",")
            {
                nonDecimalSeparator = ".";
            }

            entry.Text = entry.Text.Replace(nonDecimalSeparator, CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);
        }
    }
}
