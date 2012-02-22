#region using directives

using System;

#endregion

namespace AsciiImportExport
{
    public class ImportException : Exception
    {
        public ImportException(string columnName, Exception ex)
            : base(columnName, ex)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; set; }
    }
}