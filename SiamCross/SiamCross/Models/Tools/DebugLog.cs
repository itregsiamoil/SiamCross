using System;
using System.Diagnostics;
using System.Text;

namespace SiamCross.Models.Tools
{
    public static class DebugLog
    {
        [Conditional("DEBUG_UNIT")]
        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }
        [Conditional("DEBUG_UNIT")]
        public static void WriteLine(object value, string category)
        {
            Debug.WriteLine(value, category);
        }
        [Conditional("DEBUG_UNIT")]
        public static void WriteLine(object value)
        {
            Debug.WriteLine(value);
        }
        [Conditional("DEBUG_UNIT")]
        public static void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }
    };


}
