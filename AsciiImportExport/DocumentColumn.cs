#region using directives

using System;
using System.Globalization;
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
        private string _doubleStringFormat = "0.0000";
        private readonly bool _dummyColumn;
        private readonly Func<T, object> _getValueFunc;
        private string _header;
        private readonly Action<T, object> _setValueFunc;
        private Func<object, string> _exportFunc;
        private Func<string, object> _importFunc;
        private readonly Type type;

        public DocumentColumn(Expression<Func<T, object>> expression)
        {
            _getValueFunc = expression.Compile();

            MemberExpression me = ReflectionHelper.GetMemberExpression(expression);
            if (me != null)
            {
                _header = me.Member.Name;
                var propertyInfo = me.Member as PropertyInfo;
                type = propertyInfo.PropertyType;
                _setValueFunc = GetValueSetter(propertyInfo);
            }
            else _dummyColumn = true;

            ColumnWidth = -1;
        }

        public ColumnAlignment Alignment
        {
            get { return _alignment; }
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

            object columnValue = _getValueFunc.Invoke(item);

            if (columnValue == null && _defaultValue != null)
                columnValue = _defaultValue;

            if (_exportFunc == null)
            {
                if (type == typeof(String)) _exportFunc = value => FormatAsString((string)value);
                else if (type == typeof(Int32) || type == typeof(Int32?)) _exportFunc = value => FormatAsInteger((int?)value);
                else if (type == typeof(Double) || type == typeof(Double?)) _exportFunc = value => FormatAsDouble((double?)value);
                else if (type == typeof(Boolean) || type == typeof(Boolean?)) _exportFunc = value => FormatAsBoolean((Boolean?)value);
                else if (type == typeof(DateTime) || type == typeof(DateTime?)) _exportFunc = value => FormatAsDateTime((DateTime?)value);
                else throw new InvalidOperationException("the column type '" + type.FullName + "' is unknown");
            }

            return _exportFunc(columnValue);
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
            _doubleStringFormat = "0.".PadRight(precision + 2, '0');
            return this;
        }

        public DocumentColumn<T> SetHeader(string header)
        {
            _header = header;
            return this;
        }

        public DocumentColumn<T> SetImportExportActions(Expression<Func<string, object>> importAction, Expression<Func<object, string>> exportAction)
        {
            if (importAction != null) _importFunc = importAction.Compile();
            if (exportAction != null) _exportFunc = exportAction.Compile();

            return this;
        }

        public void SetValue(T item, string valueString)
        {
            if (_dummyColumn) return;
            if (valueString == null) return;

            if(_importFunc == null)
            {
                if (type == typeof(String)) _importFunc = s => s;
                else if (type == typeof(int) || type == typeof(int?)) _importFunc = s => ConvertToInteger(s);
                else if (type == typeof(double) || type == typeof(double?)) _importFunc = s => ConvertToDouble(s);
                else if (type == typeof(DateTime) || type == typeof(DateTime?)) _importFunc = s => ConvertToDateTime(s);
                else if (type == typeof(Boolean) || type == typeof(Boolean?)) _importFunc = s => ConvertToBoolean(s);
                else throw new InvalidOperationException("the column type '" + type.FullName + "' is unknown");   
            }

            _setValueFunc(item, _importFunc(valueString));
        }

        public override string ToString()
        {
            return _dummyColumn ? "DummyColumn" : ((_header ?? "")) + " [" + type + "]";
        }

        private bool ConvertToBoolean(string valueString)
        {
            if (valueString == _booleanTrue) return true;
            if (valueString == _booleanFalse) return false;

            if (_defaultValue != null) return (bool) _defaultValue;

            throw new InvalidOperationException("invalid value '" + valueString + "' in boolean column '" + _header + "'");
        }

        private DateTime ConvertToDateTime(string value)
        {
            return value == null ? DateTime.MinValue : DateTime.ParseExact(value, _dateTimeFormat, CultureInfo.InvariantCulture.DateTimeFormat);
        }

        private double? ConvertToDouble(string value)
        {
            return String.IsNullOrEmpty(value) ? (double?) _defaultValue : Double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
        }

        private int? ConvertToInteger(string value)
        {
            return String.IsNullOrEmpty(value) ? (int?) _defaultValue : Int32.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
        }

        private string FormatAsBoolean(bool? value)
        {
            return FormatAsString(!value.HasValue ? "" : (value.Value ? _booleanTrue : _booleanFalse));
        }

        private string FormatAsDateTime(DateTime? value)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString(_dateTimeFormat, CultureInfo.InvariantCulture.DateTimeFormat));
        }

        private string FormatAsDouble(double? value)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString(_doubleStringFormat, CultureInfo.InvariantCulture.NumberFormat));
        }

        private string FormatAsInteger(int? value)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        /// <summary>
        /// From ServiceStack.Text - StaticAccessors.cs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private static Action<T, object> GetValueSetter(PropertyInfo propertyInfo)
        {
            if (typeof (T) != propertyInfo.DeclaringType)
            {
                throw new ArgumentException();
            }

            ParameterExpression instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            ParameterExpression argument = Expression.Parameter(typeof (object), "a");
            MethodCallExpression setterCall = Expression.Call(
                instance,
                propertyInfo.GetSetMethod(),
                Expression.Convert(argument, propertyInfo.PropertyType));

            return Expression.Lambda<Action<T, object>>
                (
                    setterCall, instance, argument
                ).Compile();
        }
    }
}