using System;

namespace SiamCross.Models.Tools
{
    public static class CheckValue
    {
        public static T MinMax<T>(T min, T max, T val) where T : IComparable
        {
            if (0 > val.CompareTo(min))
                return min;
            else if (0 < val.CompareTo(max))
                return max;
            return val;
        }
    }
}
