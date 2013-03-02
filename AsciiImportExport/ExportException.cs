using System;

namespace AsciiImportExport
{
    /// <summary>
    /// This exception carries parsing error information
    /// </summary>
    public class ExportException : Exception
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="columnName">The column where the error happend</param>
        /// <param name="item">The item that caused the error</param>
        /// <param name="ex">The original exception</param>
        /// <param name="message">Optional message</param>
        public ExportException(string columnName, object item, Exception ex, string message = null)
            : base(message, ex)
        {
            ColumnName = columnName;
            Item = item;
        }

        /// <summary>
        /// The column where the error happend
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The item that caused the export error
        /// </summary>
        public object Item { get; set; }
    }
}