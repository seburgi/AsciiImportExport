#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AsciiImportExport
{
    public class DocumentFormatDefinition<T> where T : class, new()
    {
        private readonly bool _autosizeColumns;
        private readonly List<DocumentColumn<T>> _columns;
        private readonly bool _exportHeaderLine;
        private readonly Func<T> _instantiator;

        public DocumentFormatDefinition(List<DocumentColumn<T>> columns, string columnSeparator, string commentString, bool autosizeColumns, bool exportHeaderLine, Func<T> instantiator)
        {
            _columns = columns;

            ColumnSeparator = columnSeparator;
            CommentString = commentString;

            _autosizeColumns = autosizeColumns;
            _exportHeaderLine = exportHeaderLine;
            _instantiator = instantiator;
        }

        public string ColumnSeparator { get; private set; }

        public IEnumerable<DocumentColumn<T>> Columns
        {
            get { return _columns; }
        }

        public string CommentString { get; private set; }

        public string Export(IEnumerable<T> data)
        {
            if (!data.Any()) return "";

            var exportResults = new string[data.Count()][];

            int i = 0;
            foreach (var item in data)
            {
                exportResults[i] = new string[_columns.Count];

                for (int j = 0; j < _columns.Count; j++)
                {
                    exportResults[i][j] = _columns[j].Format(item);
                }
                i++;
            }

            if (_autosizeColumns) InitializeAutosizeColumns(exportResults);

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
            }

            for (i = 0; i < exportResults.Length; i++)
            {
                var lineSb = new StringBuilder();
                for (int j = 0; j < _columns.Count; j++)
                {
                    lineSb.Append(_columns[j].FormatAsString(exportResults[i][j]));
                    lineSb.Append(ColumnSeparator);
                }
                sb.AppendLine(lineSb.ToString().TrimEnd(ColumnSeparator.ToArray()));
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        public List<T> Import(string fileContent)
        {
            string[] lines = fileContent.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            return Import(lines);
        }

        public List<T> Import(IEnumerable<string> lines)
        {
            var result = new List<T>();

            int zeile = 0;
            string line = null;
            try
            {
                int count = lines.Count();
                DocumentColumn<T> lastColumn = _columns.Last();

                for (; zeile < count; zeile++)
                {
                    line = lines.ElementAt(zeile);

                    if (String.IsNullOrEmpty(line.Trim())) continue;
                    if (line.StartsWith(CommentString)) continue;
                    //if (!String.IsNullOrEmpty(_praefix) && line.StartsWith(_praefix)) line = line.Substring(_praefix.Length);

                    line += ColumnSeparator;

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
                                value = line.Trim();
                            }
                            else
                            {
                                line = line.TrimStart(new[] {' '});
                                int indexOfSeparator = line.IndexOf(ColumnSeparator);
                                if (indexOfSeparator < 0) break;
                                value = line.Substring(0, indexOfSeparator);
                                int nextColumnIndex = indexOfSeparator + ColumnSeparator.Length;
                                line = nextColumnIndex <= line.Length ? line.Substring(nextColumnIndex) : "";
                            }
                        }
                        column.SetValue(item, value.Trim());
                    }

                    result.Add(item);
                }
            }
            catch (ImportException ex)
            {
                throw new ImportException("Error during importing of column '" + ex.ColumnName + "' at line " + (zeile + 1) + ": '" + line + "'", ex);
            }

            return result;
        }

        private void InitializeAutosizeColumns(string[][] data)
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                if (_columns[i].ColumnWidth > 0) continue;

                int maxLength = data.Select(t => t[i].Length).Max();
                _columns[i].SetColumnWidth(maxLength);
            }
        }
    }
}