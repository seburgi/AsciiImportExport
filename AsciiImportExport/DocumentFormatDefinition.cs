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
        private readonly bool _checkForComments;
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

            CommentString = commentString ?? "";
            _checkForComments = CommentString.Length > 0;

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
            List<int> columnWidths = _columns.Select(x => x.ColumnWidth).ToList();

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

                    if (_exportHeaderLine)
                        maxLength = Math.Max(maxLength, _columns[i].Header.Length);

                    columnWidths[i] = Math.Max(columnWidths[i], maxLength);
                }
            }

            var sb = new StringBuilder();

            // Write header line
            if (_exportHeaderLine)
            {
                var lineSb = new StringBuilder();

                for (int i = 0; i < _columns.Count; i++)
                {
                    lineSb.Append(_columns[i].Format(_columns[i].Header, columnWidths[i]));
                    lineSb.Append(ColumnSeparator);
                }
                sb.AppendLine(lineSb.ToString().TrimEnd(ColumnSeparator.ToArray()));
            }

            for (int i = 0; i < exportResults.Length; i++)
            {
                var lineSb = new StringBuilder();
                for (int j = 0; j < _columns.Count; j++)
                {
                    lineSb.Append(_autosizeColumns ? _columns[j].Format(exportResults[i][j], columnWidths[j]) : _columns[j].Format(exportResults[i][j], _columns[j].ColumnWidth));

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
            int lineNr = -1;

            try
            {
                var result = new List<T>();

                foreach (var line in lines)
                {
                    lineNr++;

                    int linePos = 0;
                    int lineLength = line.Length;

                    if (_checkForComments)
                    {
                        int commentPos = line.IndexOf(CommentString, StringComparison.InvariantCulture);
                        if (commentPos >= 0)
                            lineLength = commentPos;
                    }

                    if (lineLength == 0) continue;
                    while (linePos < lineLength)
                    {
                        if (line[linePos] == ' ')
                            linePos++;
                        else
                            break;
                    }
                    if (linePos == lineLength) continue;

                    linePos = 0;
                    T item = _instantiator();

                    foreach (var column in _columns)
                    {
                        if (linePos >= lineLength) break;

                        string value;
                        if (column.ColumnWidth > 0)
                        {
                            value = line.Substring(linePos, Math.Min(column.ColumnWidth, lineLength - linePos));
                            linePos += column.ColumnWidth + ColumnSeparator.Length;
                        }
                        else
                        {
                            while (linePos < lineLength)
                            {
                                if (line[linePos] == ' ')
                                    linePos++;
                                else
                                    break;
                            }

                            int indexOfSeparator = line.IndexOf(ColumnSeparator, linePos);
                            if (indexOfSeparator >= 0)
                            {
                                value = line.Substring(linePos, indexOfSeparator - linePos);
                                linePos = indexOfSeparator + ColumnSeparator.Length;
                            }
                            else
                            {
                                value = line.Substring(linePos);
                                linePos = lineLength;
                            }
                        }
                        column.Parse(item, value.Trim());
                    }

                    result.Add(item);
                }

                return result;
            }
            catch (ImportException ex)
            {
                throw new ImportException(ex.ColumnName, ex.Value, ex, "Error during parsing of column '" + ex.ColumnName + "' at line " + (lineNr + 1) + ": column value '" + ex.Value + "'");
            }
        }
    }
}