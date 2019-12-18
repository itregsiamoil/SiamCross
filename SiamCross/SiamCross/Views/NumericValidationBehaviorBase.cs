using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xamarin.Forms;

namespace SiamCross.Views
{
    public abstract class NumericValidationBehaviorBase : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        protected abstract void OnEntryTextChanged(object sender, TextChangedEventArgs args);
        //{
        //    if (!string.IsNullOrWhiteSpace(args.NewTextValue))
        //    {
        //        bool isValid =
        //            args.NewTextValue.ToCharArray().All(x => char.IsDigit(x));
        //        ((Entry)sender).Text =
        //            isValid ? args.NewTextValue : args.NewTextValue.Remove(
        //                args.NewTextValue.Length - 1);
        //    }
        //}
    }
}
