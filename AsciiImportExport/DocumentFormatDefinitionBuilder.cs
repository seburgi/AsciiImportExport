#region using directives

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace AsciiImportExport
{
    public class DocumentFormatDefinitionBuilder<T> where T : class, new()
    {
        private bool _autosizeColumns;
        private string _columnSeparator = ";";
        private readonly List<IDocumentColumn<T>> _columns = new List<IDocumentColumn<T>>();
        private string _commentString = "'";
        private bool _exportHeaderLine;
        private Func<T> _instantiator = () => new T();

        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, double?>> expression, string doubleStringFormat, Action<DocumentColumnBuilder<T, double?>> customization = null)
        {
            DocumentColumnBuilder<T, double?> builder = new DocumentColumnBuilder<T, double?>(expression).SetDoubleStringFormat(doubleStringFormat);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, DateTime?>> expression, string dateTimeStringFormat, Action<DocumentColumnBuilder<T, DateTime?>> customization = null)
        {
            DocumentColumnBuilder<T, DateTime?> builder = new DocumentColumnBuilder<T, DateTime?>(expression).SetDateTimeStringFormat(dateTimeStringFormat);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, bool?>> expression, string trueString, string falseString, Action<DocumentColumnBuilder<T, bool?>> customization = null)
        {
            DocumentColumnBuilder<T, bool?> builder = new DocumentColumnBuilder<T, bool?>(expression).SetBooleanStrings(trueString, falseString);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn(IDocumentColumn<T> column)
        {
            _columns.Add(column);
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddColumn<TRet>(Expression<Func<T, TRet>> expression, Action<DocumentColumnBuilder<T, TRet>> customization = null)
        {
            var builder = new DocumentColumnBuilder<T, TRet>(expression);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddDummyColumn(string exportValue = "")
        {
            return AddColumn(x => "", builder => builder.SetExportFunc(x => exportValue));
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