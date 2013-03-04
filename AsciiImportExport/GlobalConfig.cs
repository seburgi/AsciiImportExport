using System;
using System.Globalization;

namespace AsciiImportExport
{
    /// <summary>
    /// Contains default information for <see cref="IDocumentColumn{T}"/>
    /// </summary>
    public static class GlobalConfig
    {
        /// <summary>
        /// Default boolean false string (Default = "F")
        /// </summary>
        public static string DefaultBooleanFalseString = "F";

        /// <summary>
        /// Default boolean true string (Default = "T")
        /// </summary>
        public static string DefaultBooleanTrueString = "T";

        /// <summary>
        /// Default StringFormat for DateTime (Default = null)
        /// </summary>
        public static string DefaultDatetimeStringFormat = null;

        /// <summary>
        /// Default IFormatProvider (Default = CultureInfo.InvariantCulture)
        /// </summary>
        public static IFormatProvider DefaultFormatProvider = CultureInfo.InvariantCulture;

        /// <summary>
        /// Default StringFormat for numeric types (Default = null)
        /// </summary>
        public static string DefaultNumericStringFormat = null;
    }
}