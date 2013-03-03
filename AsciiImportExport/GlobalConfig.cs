using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AsciiImportExport
{
    /// <summary>
    /// Contains default information for <see cref="IDocumentColumn{T}"/>
    /// </summary>
    public static class GlobalConfig
    {
        /// <summary>
        /// Default StringFormat for DateTime (Default = null)
        /// </summary>
        public static string DefaultDatetimeStringFormat = null;

        /// <summary>
        /// Default StringFormat for numeric types (Default = null)
        /// </summary>
        public static string DefaultNumericStringFormat = null;

        /// <summary>
        /// Default IFormatProvider  (Default = CultureInfo.InvariantCulture)
        /// </summary>
        public static IFormatProvider DefaultFormatProvider = CultureInfo.InvariantCulture;
    }
}
