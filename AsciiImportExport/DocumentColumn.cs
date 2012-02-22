#region using directives

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace AsciiImportExport
{
    public class DocumentColumn<T>
        where T : class, new()
    {
        private ColumnAlignment _alignment = ColumnAlignment.Left;
        private string _booleanFalse = "0";
        private string _booleanTrue = "1";
        private string _dateTimeFormat = "dd.MM.yyyy HH:mm:ss";
        private object _defaultValue;
        private int _doublePrecision = 4;
        private readonly bool _dummyColumn;
        private readonly Func<T, object> _getValueFunc;
        private string _header;
        private readonly PropertyInfo _propertyInfo;
        private Func<object, string> _specialExportFunc;
        private Func<string, object> _specialImportFunc;

        public DocumentColumn(Expression<Func<T, object>> expression)
        {
            _getValueFunc = expression.Compile();

            MemberExpression me = ReflectionHelper.GetMemberExpression(expression);
            if (me != null)
            {
                _header = me.Member.Name;
                _propertyInfo = me.Member as PropertyInfo;
            }
            else _dummyColumn = true;

            ColumnWidth = -1;
        }

        public int ColumnWidth { get; private set; }

        public string FormattedHeader
        {
            get
            {
                string header = FormatAsString(_header);
                //if (_isFirst && header.Length > 0)
                //{
                //    if (_alignment == ColumnAlignment.Right && header.First() == ' ') header = header.Substring(1);
                //    else if (_alignment == ColumnAlignment.Left && header.Last() == ' ') header = header.Substring(0, header.Length - 1);
                //    header = _commentString + header;
                //}
                return header;
            }
        }

        public string Header
        {
            get { return _header; }
        }

        public string Format(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            object value = _getValueFunc.Invoke(item);

            if (_specialExportFunc != null) return FormatAsString(_specialExportFunc.Invoke(value));

            if (value == null)
                if (_defaultValue == null) return FormatAsString("");
                else value = _defaultValue;

            Type type = value.GetType();
            if (type == typeof (String)) return FormatAsString((string) value);
            if (type == typeof (int) || type == typeof (int?)) return FormatAsInteger((int?) value);
            if (type == typeof (double) || type == typeof (double?)) return FormatAsDouble((double?) value, _doublePrecision);
            if (type == typeof (Boolean) || type == typeof (Boolean?)) return FormatAsBoolean((Boolean?) value);
            if (type == typeof (DateTime) || type == typeof (DateTime?)) return FormatAsDateTime((DateTime?) value);

            throw new InvalidOperationException("the column type '" + type.FullName + "' is unknown");
        }

        public string FormatAsString(string str)
        {
            int width = ColumnWidth >= 0 ? ColumnWidth : 0;

            return (_alignment == ColumnAlignment.Right ? str.PadLeft(width) : str.PadRight(width));
        }

        public DocumentColumn<T> SetAlignment(ColumnAlignment alignment)
        {
            _alignment = alignment;
            return this;
        }

        public DocumentColumn<T> SetBooleanStrings(string trueString, string falseString)
        {
            _booleanTrue = trueString;
            _booleanFalse = falseString;
            return this;
        }

        public DocumentColumn<T> SetColumnWidth(int columnWidth)
        {
            ColumnWidth = columnWidth;
            return this;
        }

        public DocumentColumn<T> SetDateTimeFormat(string dateTimeFormat)
        {
            _dateTimeFormat = dateTimeFormat;
            return this;
        }

        public DocumentColumn<T> SetDefaultValue(object value)
        {
            _defaultValue = value;
            return this;
        }

        public DocumentColumn<T> SetDoublePrecision(int precision)
        {
            _doublePrecision = precision;
            return this;
        }

        public DocumentColumn<T> SetHeader(string header)
        {
            _header = header;
            return this;
        }

        public DocumentColumn<T> SetImportExportActions(Expression<Func<string, object>> importAction, Expression<Func<object, string>> exportAction)
        {
            if (importAction != null) _specialImportFunc = importAction.Compile();
            if (exportAction != null) _specialExportFunc = exportAction.Compile();

            return this;
        }

        public void SetValue(T item, string valueString)
        {
            if (_dummyColumn) return;
            if (valueString == null) return;

            if (_specialImportFunc != null)
            {
                _propertyInfo.SetValue(item, _specialImportFunc.Invoke(valueString), null);
                return;
            }

            try
            {
                Type type = _propertyInfo.PropertyType;
                if (type == typeof (String)) _propertyInfo.SetValue(item, valueString, null);
                else if (type == typeof (int) || type == typeof (int?)) _propertyInfo.SetValue(item, ConvertToInteger(valueString), null);
                else if (type == typeof (double) || type == typeof (double?)) _propertyInfo.SetValue(item, ConvertToDouble(valueString), null);
                else if (type == typeof (DateTime) || type == typeof (DateTime?)) _propertyInfo.SetValue(item, ConvertToDateTime(valueString, _dateTimeFormat, DateTime.MinValue), null);
                else if (type == typeof (Boolean) || type == typeof (Boolean?)) _propertyInfo.SetValue(item, ConvertToBoolean(valueString), null);
                else throw new InvalidOperationException("the column type '" + type.FullName + "' is unknown");
            }
            catch (Exception ex)
            {
                throw new ImportException(_header, ex);
            }
        }

        public override string ToString()
        {
            return _dummyColumn ? "DummyColumn" : ((_header ?? "")) + " [" + _propertyInfo.PropertyType + "]";
        }

        private bool ConvertToBoolean(string valueString)
        {
            if (valueString == _booleanTrue) return true;
            if (valueString == _booleanFalse) return false;

            if (_defaultValue != null) return (bool) _defaultValue;

            throw new InvalidOperationException("invalid value '" + valueString + "' in boolean column '" + _header + "'");
        }

        private DateTime ConvertToDateTime(string value, string format, DateTime defaultValue)
        {
            return value == null ? defaultValue : DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }

        private double? ConvertToDouble(string value)
        {
            return String.IsNullOrEmpty(value) ? (double?) _defaultValue : Double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        private int? ConvertToInteger(string value)
        {
            return String.IsNullOrEmpty(value) ? (int?) _defaultValue : Int32.Parse(value, CultureInfo.InvariantCulture);
        }

        private string FormatAsBoolean(bool? value)
        {
            return FormatAsString(!value.HasValue ? "" : (value.Value ? _booleanTrue : _booleanFalse));
        }

        private string FormatAsDateTime(DateTime? value)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString(_dateTimeFormat, CultureInfo.InvariantCulture));
        }

        private string FormatAsDouble(double? value, int precision)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString("0.".PadRight(precision + 2, '0'), CultureInfo.InvariantCulture));
        }

        private string FormatAsInteger(int? value)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString());
        }
    }
}