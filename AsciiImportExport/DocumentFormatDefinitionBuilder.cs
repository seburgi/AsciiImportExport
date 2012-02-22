#region using directives

using System;
using System.Collections.Generic;

#endregion

namespace AsciiImportExport
{
    public class DocumentFormatDefinitionBuilder<T> where T : class, new()
    {
        private bool _autosizeColumns = false;
        private string _columnSeparator = ";";
        private readonly List<DocumentColumn<T>> _columns = new List<DocumentColumn<T>>();
        private string _commentString = "'";
        private bool _exportHeaderLine = false;
        private Func<T> _instantiator = () => new T();

        public DocumentFormatDefinitionBuilder<T> AddColumn(DocumentColumn<T> column)
        {
            _columns.Add(column);
            return this;
        }

        public DocumentFormatDefinitionBuilder<T> AddDummyColumn(string exportValue = "")
        {
            return AddColumn(new DocumentColumn<T>(x => x).SetImportExportActions(null, x => exportValue));
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