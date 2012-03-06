#region using directives

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace AsciiImportExport
{
    public class DocumentFormatDefinitionBuilder<T> where T : class, new()
    {
        private bool _autosizeColumns = false;
        private string _columnSeparator = ";";
        private readonly List<IDocumentColumn<T>> _columns = new List<IDocumentColumn<T>>();
        private string _commentString = "'";
        private bool _exportHeaderLine = false;
        private Func<T> _instantiator = () => new T();

        public DocumentFormatDefinitionBuilder<T> AddColumn<TRet>(Expression<Func<T, TRet>> expression, ColumnAlignment alignment = ColumnAlignment.Left, Func<string, TRet> importFunc = null, Func<TRet, string> exportFunc = null)
        {
            DocumentColumn<T, TRet> documentColumn = new DocumentColumn<T, TRet>(expression).SetAlignment(alignment);

            if (importFunc != null && exportFunc != null)
                documentColumn.SetImportExportActions(importFunc, exportFunc);

            _columns.Add(documentColumn);
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, double?>> expression, ColumnAlignment alignment = ColumnAlignment.Left, string doubleStringFormat = "0.0000")
        {
            _columns.Add(new DocumentColumn<T, double?>(expression).SetAlignment(alignment).SetDoubleStringFormat(doubleStringFormat));
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, DateTime?>> expression, string dateTimeFormat = "dd.MM.yyyy HH:mm:ss")
        {
            _columns.Add(new DocumentColumn<T, DateTime?>(expression).SetDateTimeStringFormat(dateTimeFormat));
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, bool?>> expression, string trueString = "T", string falseString = "F")
        {
            _columns.Add(new DocumentColumn<T, bool?>(expression).SetBooleanStrings(trueString, falseString));
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn(IDocumentColumn<T> column)
        {
            _columns.Add(column);
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddDummyColumn(string exportValue = "")
        {
            return AddColumn(new DocumentColumn<T, string>(x => "").SetImportExportActions(null, x => exportValue));
        }

        public DocumentFormatDefinition<T> Build()
        {
            return new DocumentFormatDefinition<T>(_columns, _columnSeparator, _commentString, _autosizeColumns, _exportHeaderLine, _instantiator);
        }

        public DocumentFormatDefinitionBuilder<T> SetAutosizeColumns(bool autosizeColumns)
        {
            _autosizeColumns = autosizeColumns;
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> SetColumnSeparator(string columnSeparator)
        {
            _columnSeparator = columnSeparator;
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> SetCommentString(string commentString)
        {
            _commentString = commentString;
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> SetExportHeaderLine(bool exportHeaderLine)
        {
            _exportHeaderLine = exportHeaderLine;
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> SetInstantiator(Func<T> func)
        {
            _instantiator = func;
            return this;
        }
    }
}