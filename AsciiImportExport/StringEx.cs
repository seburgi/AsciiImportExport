namespace AsciiImportExport
{
    /// <summary>
    /// String extension methods
    /// </summary>
    public static class StringEx
    {
        /// <summary>
        /// Removes all occurences of an element of the given string array from the end of the input string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="trimStrings">The string array</param>
        /// <returns></returns>
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