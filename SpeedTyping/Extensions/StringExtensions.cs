using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SpeedTyping
{
    public static class StringExtensions
    {
        public static string Multiply(this string input, int times)
        {
            StringBuilder sb = new StringBuilder(input.Length * times);
            for (int i = 0; i < times; i++)
            {
                sb.Append(input);
            }
            return sb.ToString();
        }
    }
}