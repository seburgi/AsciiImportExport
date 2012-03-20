#region using directives

using System;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    /// This exception carries parsing error information
    /// </summary>
    public class ImportException : Exception
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="columnName">The column where the error happend</param>
        /// <param name="value">The value that caused the parsing error</param>
        /// <param name="ex">The original exception</param>
        /// <param name="message">Optional message</param>
        public ImportException(string columnName, string value, Exception ex, string message = null)
            : base(message, ex)
        {
            ColumnName = columnName;
            Value = value;
        }

        /// <summary>
        /// The column where the error happend
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The value that caused the parsing error
        /// </summary>
        public string Value { get; set; }
    }
}