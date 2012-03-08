#region using directives

using System;
using System.Linq.Expressions;

#endregion

namespace AsciiImportExport
{
    public class DocumentColumnBuilder<T, TRet> where T : class, new()
    {
        private ColumnAlignment _alignment = ColumnAlignment.Left;
        private string _booleanFalse = "F";
        private string _booleanTrue = "T";
        private int _columnWidth = -1;
        private string _dateTimeStringFormat = "dd.MM.yyyy HH:mm:ss";
        private TRet _defaultValue;
        private string _doubleStringFormat = "0.0000";
        private Func<TRet, string> _exportFunc;
        private readonly Expression<Func<T, TRet>> _expression;
        private string _header;
        private Func<string, TRet> _importFunc;

        public DocumentColumnBuilder(Expression<Func<T, TRet>> expression)
        {
            _expression = expression;
        }

        public DocumentColumn<T, TRet> Build()
        {
            return new DocumentColumn<T, TRet>(_expression, _header, _defaultValue, _columnWidth, _alignment, _doubleStringFormat, _dateTimeStringFormat, _booleanTrue, _booleanFalse, _importFunc, _exportFunc);
        }

        public DocumentColumnBuilder<T, TRet> SetAlignment(ColumnAlignment alignment)
        {
            _alignment = alignment;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetBooleanStrings(string trueString, string falseString)
        {
            _booleanTrue = trueString;
            _booleanFalse = falseString;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetColumnWidth(int columnWidth)
        {
            _columnWidth = columnWidth;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetDateTimeStringFormat(string dateTimeStringFormat)
        {
            _dateTimeStringFormat = dateTimeStringFormat;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetDefaultValue(TRet value)
        {
            _defaultValue = value;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetDoubleStringFormat(string doubleStringFormat)
        {
            _doubleStringFormat = doubleStringFormat;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetExportFunc(Func<TRet, string> exportFunc)
        {
            _exportFunc = exportFunc;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetHeader(string header)
        {
            _header = header;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetImportFunc(Func<string, TRet> importFunc)
        {
            _importFunc = importFunc;
            return this;
        }
    }
}