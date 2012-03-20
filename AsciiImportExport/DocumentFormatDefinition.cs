#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    /// Holds all the format information necessary to import/export columnbased text data
    /// </summary>
    /// <typeparam name="T">The type of the POCO you want to import/export</typeparam>
    public class DocumentFormatDefinition<T>
    {
        private readonly bool _autosizeColumns;
        private readonly List<IDocumentColumn<T>> _columns;
        private readonly bool _exportHeaderLine;
        private readonly Func<T> _instantiator;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="columns">List of columns of type <see cref="DocumentColumn{T,TRet}"/> defining the structure of a document</param>
        /// <param name="columnSeparator">String that is used to separate columns in the text</param>
        /// <param name="commentString">String that is used to identify the start of comments in the text</param>
        /// <param name="autosizeColumns">Defines if the rows of a column shall all be of the same width</param>
        /// <param name="exportHeaderLine">Defines if a header line shall be created during serialization</param>
        /// <param name="instantiator">Function that creates a new instance of type <see cref="T"/>. </param>
        public DocumentFormatDefinition(List<IDocumentColumn<T>> columns, string columnSeparator, string commentString, bool autosizeColumns, bool exportHeaderLine, Func<T> instantiator)
        {
            _columns = columns;

            ColumnSeparator = columnSeparator;
            CommentString = commentString;

            _autosizeColumns = autosizeColumns;
            _exportHeaderLine = exportHeaderLine;
            _instantiator = instantiator;
        }

        /// <summary>
        /// String that is used to separate columns in the text
        /// </summary>
        public string ColumnSeparator { get; private set; }

        /// <summary>
        /// Enumerable list of columns of type <see cref="DocumentColumn{T,TRet}"/> defining the structure of a document
        /// </summary>
        public IEnumerable<IDocumentColumn<T>> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// String that is used to identify the start of comments in the text
        /// </summary>
        public string CommentString { get; private set; }

        /// <summary>
        /// Serializes an enumerable list of type <see cref="T"/> to a string
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public string Export(IEnumerable<T> items)
        {
            int itemCount = items.Count();
            if (itemCount == 0) return "";

            var exportResults = new string[itemCount][];
            var columnWidths = new List<int>();

            for (int i = 0; i < itemCount; i++)
            {
                exportResults[i] = new string[_columns.Count];

                for (int j = 0; j < _columns.Count; j++)
                {
                    exportResults[i][j] = _columns[j].Serialize(items.ElementAt(i));
                }
            }

            if (_autosizeColumns)
            {
                for (int i = 0; i < _columns.Count; i++)
                {
                    int maxLength = exportResults.Select(t => t[i].Length).Max();
                    columnWidths.Add(maxLength);
                }
            }

            var sb = new StringBuilder();

            // Write header line
            if (_exportHeaderLine)
            {
                var lineSb = new StringBuilder();
                foreach (var column in _columns)
                {
                    lineSb.Append(column.FormattedHeader);
                    lineSb.Append(ColumnSeparator);
                }
                sb.AppendLine(lineSb.ToString().TrimEnd(ColumnSeparator.ToArray()));

                // Adjust width of first column due to additional length of comment string
                if (_autosizeColumns) columnWidths[0] = Math.Max(columnWidths[0], _columns[0].FormattedHeader.Length + CommentString.Length);
            }

            for (int i = 0; i < exportResults.Length; i++)
            {
                var lineSb = new StringBuilder();
                for (int j = 0; j < _columns.Count; j++)
                {
                    if (_autosizeColumns)
                    {
                        if (_columns[j].Alignment == ColumnAlignment.Right)
                        {
                            lineSb.Append(exportResults[i][j].PadLeft(columnWidths[j]));
                        }
                        else
                        {
                            lineSb.Append(exportResults[i][j].PadRight(columnWidths[j]));
                        }
                    }
                    else
                    {
                        lineSb.Append(exportResults[i][j]);
                    }

                    lineSb.Append(ColumnSeparator);
                }
                sb.AppendLine(lineSb.ToString().TrimEnd());
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Parses a string to a generic List of type <see cref="T"/>
        /// </summary>
        /// <param name="fileContent">The input string</param>
        /// <returns></returns>
        public List<T> Import(string fileContent)
        {
            string[] lines = fileContent.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            return Import(lines);
        }

        /// <summary>
        /// Parses an enumerable list of strings to a generic List of type <see cref="T"/>
        /// </summary>
        /// <param name="lines">The rows of a document</param>
        /// <returns></returns>
        public List<T> Import(IEnumerable<string> lines)
        {
            var result = new List<T>();

            int zeile = 0;
            string line = null;
            try
            {
                int count = lines.Count();
                IDocumentColumn<T> lastColumn = _columns.Last();

                for (; zeile < count; zeile++)
                {
                    line = lines.ElementAt(zeile);

                    if (String.IsNullOrEmpty(line.Trim())) continue;
                    if (line.StartsWith(CommentString)) continue;

                    T item = _instantiator();

                    foreach (var column in _columns)
                    {
                        if (line.Trim().Length == 0) break;
                        string value;
                        if (column.ColumnWidth > 0)
                        {
                            value = line.Substring(0, Math.Min(column.ColumnWidth, line.Length));
                            int nextColumnIndex = column.ColumnWidth + ColumnSeparator.Length;
                            line = nextColumnIndex <= line.Length ? line.Substring(nextColumnIndex) : "";
                        }
                        else
                        {
                            if (column == lastColumn)
                            {
                                value = line.TrimEnd(new[] {" ", ColumnSeparator});
                            }
                            else
                            {
                                line = line.TrimStart();
                                int indexOfSeparator = line.IndexOf(ColumnSeparator);
                                if (indexOfSeparator < 0) break;
                                value = line.Substring(0, indexOfSeparator);
                                int nextColumnIndex = indexOfSeparator + ColumnSeparator.Length;
                                line = nextColumnIndex <= line.Length ? line.Substring(nextColumnIndex) : "";
                            }
                        }
                        column.Parse(item, value.Trim());
                    }

                    result.Add(item);
                }
            }
            catch (ImportException ex)
            {
                throw new ImportException(ex.ColumnName, ex.Value, ex, "Error during parsing of column '" + ex.ColumnName + "' at line " + (zeile + 1) + ": '" + line + "' column value '" + ex.Value + "'");
            }

            return result;
        }
    }
}