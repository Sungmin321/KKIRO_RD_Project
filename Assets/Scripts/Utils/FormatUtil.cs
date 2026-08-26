using System;

namespace TDGame.Utils
{
    public static class FormatUtil
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };

        public static string FormatNumber(double value)
        {
            if (value < 0) return "-" + FormatNumber(-value);
            if (value < 1000) return value.ToString("0");

            int exp = (int)(Math.Log10(value) / 3);
            if (exp >= Suffixes.Length) exp = Suffixes.Length - 1;

            double scaled = value / Math.Pow(10, exp * 3);
            return $"{scaled:F2}{Suffixes[exp]}";
        }
    }
}