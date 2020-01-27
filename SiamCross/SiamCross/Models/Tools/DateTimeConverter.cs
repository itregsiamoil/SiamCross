using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Tools
{
    [Preserve(AllMembers = true)]
    public class DateTimeConverter
    {
        public string DateTimeToString(DateTime dateTime)
        {
            var month = dateTime.Date.Month.ToString();
            if (month.Length < 2)
            {
                month = "0" + month;
            }

            var day = dateTime.Date.Day.ToString();
            if (day.Length < 2)
            {
                day = "0" + day;
            }

            string date = dateTime.Date.Year.ToString() + "-" +
            month + "-" + day;
            string time = dateTime.TimeOfDay.ToString().Split('.')[0];

            return date + "T" + time;
        }
    }
}
