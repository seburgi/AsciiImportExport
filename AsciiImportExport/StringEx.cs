using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsciiImportExport
{
    public static class StringEx
    {
        public static string TrimEnd(this string input, string[] trimStrings)
        {
            bool repeat;
            do
            {
                repeat = false;
                foreach (var trimString in trimStrings)
                {
                    if (!input.EndsWith(trimString)) continue;

                    input = input.Substring(0, input.Length - trimString.Length);
                    repeat = true;
                    break;
                }
            } while (repeat);

            return input;
        }
    }
}
