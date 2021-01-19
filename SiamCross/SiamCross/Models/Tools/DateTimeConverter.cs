using System;

namespace SiamCross.Models.Tools
{
    public class DateTimeConverter
    {
        public string DateTimeToString(DateTime dateTime)
        {
            string month = dateTime.Date.Month.ToString();
            if (month.Length < 2)
            {
                month = "0" + month;
            }

            string day = dateTime.Date.Day.ToString();
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
