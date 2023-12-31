﻿namespace CruderSimple.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Join(this string left, params string[] right)
        {
            if (string.IsNullOrEmpty(left))
                return string.Join(",", right);
            return $"{left},{string.Join(",", right)}";
        }
        public static string Join(this bool left, params bool[] right)
        {
            if (left)
                return string.Join(",", right);
            return $"{left},{string.Join(",", right)}";
        }
    }
}
