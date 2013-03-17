using System;
using System.Reflection;

namespace AsciiImportExport
{
    internal class DocumentColumn<T> : IDocumentColumn<T>
    {
        private readonly string _booleanFalse;
        private readonly string _booleanTrue;
        private Func<T, object, string> _exportFunc;
        private readonly char _fillChar;
        private readonly Func<object> _getDefaultValueFunc;
        private readonly Func<T, object> _getValueFunc;
        private Func<string, object> _importFunc;
        private readonly bool _isDummyColumn;
        private readonly PropertyInfo _propertyInfo;
        private readonly IFormatProvider _provider;
        private Action<T, object> _setValueFunc;
        private readonly string _stringFormat;
        private readonly bool _throwOnColumnOverflow;
        private readonly bool _exportQuotedString;
        private readonly Type _type;

        public DocumentColumn(PropertyInfo propertyInfo, string header, Func<object> getDefaultValueFunc, int columnWidth, ColumnAlignment alignment, string stringFormat, IFormatProvider provider, string booleanTrue, string booleanFalse, Func<string, object> importFunc, Func<T, object, string> exportFunc, char fillChar, bool throwOnColumnOverflow, bool exportQuotedString)
        {
            _propertyInfo = propertyInfo;
            Header = header;
            _getDefaultValueFunc = getDefaultValueFunc;
            _stringFormat = stringFormat;
            _provider = provider;
            ColumnWidth = columnWidth;
            Alignment = alignment;
            _booleanTrue = booleanTrue;
            _booleanFalse = booleanFalse;
            _importFunc = importFunc;
            _exportFunc = exportFunc;
            _fillChar = fillChar;
            _throwOnColumnOverflow = throwOnColumnOverflow;
            _exportQuotedString = exportQuotedString;

            if (_propertyInfo != null)
            {
                _getValueFunc = _propertyInfo.GetValueGetter<T>();
                Header = Header ?? _propertyInfo.Name;
                _type = _propertyInfo.PropertyType;
            }
            else
            {
                _isDummyColumn = true;
                _getValueFunc = x => "";
                Header = Header ?? "";
            }
        }

        public ColumnAlignment Alignment { get; private set; }

        public int ColumnWidth { get; private set; }

        public string Header { get; set; }

        public string Format(string value, int columnWidth)
        {
            int width = columnWidth >= 0 ? columnWidth : 0;

            return (Alignment == ColumnAlignment.Right ? value.PadLeft(width, _fillChar) : value.PadRight(width, _fillChar));
        }

        public void Parse(T item, string value)
        {
            // Remove unneeded quotes
            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
            {
                value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
            }
            
            if (_isDummyColumn) return;
            if (_importFunc == null)
            {
                _importFunc = s => ServiceStackTextHelpers.GetParseFn(_propertyInfo.PropertyType, _stringFormat, _booleanTrue, _provider)(s);
            }

            try
            {
                object v;

                if (String.IsNullOrEmpty(value))
                {
                    if (_getDefaultValueFunc == null) return;

                    v = _getDefaultValueFunc();
                }
                else
                {
                    v = _importFunc(value);
                }

                if (_setValueFunc == null)
                {
                    _setValueFunc = _propertyInfo.GetValueSetter<T>();
                }

                _setValueFunc(item, v);
            }
            catch (Exception ex)
            {
                throw new ImportException(Header, value, ex);
            }
        }

        public string Serialize(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            object value = _getValueFunc.Invoke(item);

            if (value == null && _getDefaultValueFunc != null) value = _getDefaultValueFunc();
            if (value == null) return "";

            if (_exportFunc == null)
            {
                _exportFunc = (x, ret) => ServiceStackTextHelpers.GetSerializeFunc(_propertyInfo.PropertyType, _stringFormat, _booleanTrue, _booleanFalse, _provider, _exportQuotedString)(ret);
            }

            try
            {
                string exportedValue = _exportFunc(item, value);

                if (_throwOnColumnOverflow && exportedValue.Length > ColumnWidth)
                    throw new OverflowException(string.Format("Column data width ({0}) exceeded maximum column width ({1})", exportedValue.Length, ColumnWidth));

                return exportedValue;
            }
            catch (Exception ex)
            {
                throw new ExportException(Header, item, ex);
            }
        }

        public override string ToString()
        {
            return _isDummyColumn ? "DummyColumn" : ((Header ?? "")) + " [" + _type + "]";
        }
    }
}