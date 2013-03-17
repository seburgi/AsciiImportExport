using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AsciiImportExport
{
    internal class DocumentFormatDefinition<T> : IDocumentFormatDefinition<T>
    {
        private readonly bool _autosizeColumns;
        private readonly bool _checkForComments;
        private readonly List<IDocumentColumn<T>> _columns;
        private readonly bool _exportHeaderLine;
        private readonly string _headerLinePraefix;
        private readonly Func<T> _instantiator;
        private readonly bool _lineEndsWithColumnSeparator;
        private readonly bool _trimLineEnds;

        public DocumentFormatDefinition(List<IDocumentColumn<T>> columns, string columnSeparator, string commentString, bool autosizeColumns, bool exportHeaderLine, string headerLinePraefix, Func<T> instantiator, bool lineEndsWithColumnSeparator, bool trimLineEnds)
        {
            _columns = columns;

            ColumnSeparator = columnSeparator;

            CommentString = commentString ?? "";
            _checkForComments = CommentString.Length > 0;

            _autosizeColumns = autosizeColumns;
            _exportHeaderLine = exportHeaderLine;
            _headerLinePraefix = headerLinePraefix ?? "";
            _instantiator = instantiator;
            _lineEndsWithColumnSeparator = lineEndsWithColumnSeparator;
            _trimLineEnds = trimLineEnds;
        }

        public string ColumnSeparator { get; private set; }

        public IEnumerable<IDocumentColumn<T>> Columns
        {
            get { return _columns; }
        }

        public string CommentString { get; private set; }

        public string Export(IEnumerable<T> items)
        {
            int itemCount = items.Count();
            if (itemCount == 0) return "";

            var exportResults = new string[itemCount][];
            List<int> columnWidths = _columns.Select(x => x.ColumnWidth).ToList();

            int itemNr = 0;
            try
            {
                for (itemNr = 0; itemNr < itemCount; itemNr++)
                {
                    exportResults[itemNr] = new string[_columns.Count];

                    for (int j = 0; j < _columns.Count; j++)
                    {
                        exportResults[itemNr][j] = _columns[j].Serialize(items.ElementAt(itemNr));
                    }
                }
            }
            catch (ExportException ex)
            {
                throw new ExportException(ex.ColumnName, ex.Item, ex, "Error during export of column '" + ex.ColumnName + "' of item " + itemNr);
            }

            if (_autosizeColumns)
            {
                for (int i = 0; i < _columns.Count; i++)
                {
                    int maxLength = exportResults.Select(t => t[i].Length).Max();

                    if (_exportHeaderLine)
                    {
                        maxLength = i == 0 ? Math.Max(maxLength, _headerLinePraefix.Length + _columns[i].Header.Length) : Math.Max(maxLength, _columns[i].Header.Length);
                    }

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
                    if (i == 0)
                    {
                        lineSb.Append(_headerLinePraefix);
                        lineSb.Append(_columns[i].Format(_columns[i].Header, columnWidths[i] - _headerLinePraefix.Length));
                    }
                    else
                    {
                        lineSb.Append(_columns[i].Format(_columns[i].Header, columnWidths[i]));
                    }

                    if (i < _columns.Count - 1 || _lineEndsWithColumnSeparator)
                        lineSb.Append(ColumnSeparator);
                }
                sb.AppendLine(lineSb.ToString().TrimEnd());
            }

            for (int i = 0; i < exportResults.Length; i++)
            {
                var lineSb = new StringBuilder();
                for (int j = 0; j < _columns.Count; j++)
                {
                    lineSb.Append(_autosizeColumns ? _columns[j].Format(exportResults[i][j], columnWidths[j]) : _columns[j].Format(exportResults[i][j], _columns[j].ColumnWidth));

                    if (j < _columns.Count - 1 || _lineEndsWithColumnSeparator)
                        lineSb.Append(ColumnSeparator);
                }
                string line = lineSb.ToString();
                if (_trimLineEnds)
                {
                    line = line.TrimEnd();
                }
                sb.AppendLine(line);
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        public List<T> Import(TextReader reader, int skipLines = 0)
        {
            int lineNr = -1;

            try
            {
                var result = new List<T>();

                var columns = new List<IDocumentColumn<T>>();


                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();

                    lineNr++;

                    if (skipLines > lineNr) continue;

                    int posInLine = 0;
                    int lineLength = line.Length;

                    if (_checkForComments)
                    {
                        int commentPos = line.IndexOf(CommentString, StringComparison.InvariantCulture);

                        // If comment was found, decrease line length
                        if (commentPos >= 0)
                            lineLength = commentPos;
                    }

                    // if line length is 0, we can jump to next line
                    if (lineLength == 0) continue;

                    // check if line only consists of white spaces
                    while (posInLine < lineLength)
                    {
                        if (line[posInLine] == ' ')
                            posInLine++;
                        else
                            break;
                    }
                    
                    // if empty line, jump to next line
                    if (posInLine == lineLength) continue;

                    // now that we confirmed that the line is not empty, we can start the import
                    posInLine = 0;
                    T item = _instantiator();

                    foreach (var column in _columns)
                    {
                        if (posInLine >= lineLength) break;

                        string value;
                        if (column.ColumnWidth > 0)
                        {
                            value = line.Substring(posInLine, Math.Min(column.ColumnWidth, lineLength - posInLine));
                            posInLine += column.ColumnWidth + ColumnSeparator.Length;
                        }
                        else
                        {
                            while (posInLine < lineLength)
                            {
                                if (line[posInLine] == ' ')
                                    posInLine++;
                                else
                                    break;
                            }

                            int posOfSeparator = 0;
                            int tmp = posInLine;
                            bool isColumnSeparator = true;
                            
                            do
                            {
                                posOfSeparator = line.IndexOf(ColumnSeparator, posInLine);
                                if(posOfSeparator < 0) break;

                                for (; posInLine < posOfSeparator; posInLine++)
                                {
                                    if (line[posInLine] == '"')
                                        isColumnSeparator = !isColumnSeparator;
                                }

                                posInLine++;
                            } while (!isColumnSeparator && posInLine < lineLength);

                            posInLine = tmp;
                            if (posOfSeparator >= 0)
                            {
                                value = line.Substring(posInLine, posOfSeparator - posInLine);
                                posInLine = posOfSeparator + ColumnSeparator.Length;
                            }
                            else
                            {
                                value = line.Substring(posInLine);
                                posInLine = lineLength;
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

        public List<T> Import(string text, int skipLines = 0)
        {
            return Import(new StringReader(text), skipLines);
        }
    }
}