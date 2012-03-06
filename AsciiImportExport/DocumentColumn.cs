#region using directives

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using AsciiImportExport;

#endregion

namespace AsciiImportExport
{
    public interface IDocumentColumn<T>
    {
        ColumnAlignment Alignment { get; }
        int ColumnWidth { get; }
        string FormattedHeader { get; }
        string Header { get; }
        string Format(T item);
        string FormatAsString(string str);
        void SetValue(T item, string valueString);
    }

    public class DocumentColumn<T, TRet> : IDocumentColumn<T> where T : class, new()
    {
        private ColumnAlignment _alignment = ColumnAlignment.Left;
        private string _booleanFalse = "F";
        private string _booleanTrue = "T";
        private string _dateTimeFormat = "dd.MM.yyyy HH:mm:ss";
        private object _defaultValue;
        private string _doubleStringFormat = "0.0000";
        private readonly bool _dummyColumn;
        private Func<TRet, string> _exportFunc;
        private readonly Func<T, TRet> _getValueFunc;
        private string _header;
        private Func<string, TRet> _importFunc;
        private readonly Action<T, TRet> _setValueFunc;
        private readonly Type _type;

        public DocumentColumn(Expression<Func<T, TRet>> expression)
        {
            _getValueFunc = expression.Compile();

            MemberExpression me = ReflectionHelper.GetMemberExpression(expression);
            if (me != null)
            {
                _header = me.Member.Name;
                var propertyInfo = me.Member as PropertyInfo;
                _type = propertyInfo.PropertyType;
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

            if (columnValue == null && _defaultValue != null) columnValue = _defaultValue;

            if (_exportFunc == null)
            {
                if (_type == typeof (String)) _exportFunc = value => FormatAsString(value as string);
                else if (_type == typeof (Int32) || _type == typeof (Int32?)) _exportFunc = value => FormatAsInteger(value as int?);
                else if (_type == typeof (Double) || _type == typeof (Double?)) _exportFunc = value => FormatAsDouble(value as double?);
                else if (_type == typeof (Boolean) || _type == typeof (Boolean?)) _exportFunc = value => FormatAsBoolean(value as Boolean?);
                else if (_type == typeof (DateTime) || _type == typeof (DateTime?)) _exportFunc = value => FormatAsDateTime(value as DateTime?);
                else throw new InvalidOperationException("the column type '" + _type.FullName + "' is unknown");
            }

            return _exportFunc((TRet) columnValue);
        }

        public string FormatAsString(string str)
        {
            int width = ColumnWidth >= 0 ? ColumnWidth : 0;

            return (_alignment == ColumnAlignment.Right ? str.PadLeft(width) : str.PadRight(width));
        }

        public DocumentColumn<T, TRet> SetAlignment(ColumnAlignment alignment)
        {
            _alignment = alignment;
            return this;
        }

        public DocumentColumn<T, TRet> SetBooleanStrings(string trueString, string falseString)
        {
            _booleanTrue = trueString;
            _booleanFalse = falseString;
            return this;
        }

        public DocumentColumn<T, TRet> SetColumnWidth(int columnWidth)
        {
            ColumnWidth = columnWidth;
            return this;
        }

        public DocumentColumn<T, TRet> SetDateTimeStringFormat(string dateTimeFormat)
        {
            _dateTimeFormat = dateTimeFormat;
            return this;
        }

        public DocumentColumn<T, TRet> SetDefaultValue(object value)
        {
            _defaultValue = value;
            return this;
        }

        public DocumentColumn<T, TRet> SetDoubleStringFormat(string doubleStringFormat)
        {
            _doubleStringFormat = doubleStringFormat;
            return this;
        }

        public DocumentColumn<T, TRet> SetHeader(string header)
        {
            _header = header;
            return this;
        }

        public DocumentColumn<T, TRet> SetImportExportActions(Func<string, TRet> importFunc, Func<TRet, string> exportFunc)
        {
            _importFunc = importFunc;
            _exportFunc = exportFunc;

            return this;
        }

        public void SetValue(T item, string valueString)
        {
            if (_dummyColumn) return;
            if (valueString == null) return;

            if (_importFunc == null)
            {
                if (_type == typeof(String)) _importFunc = s => (TRet)(object)s;
                else if (_type == typeof(int) || _type == typeof(int?)) _importFunc = s => (TRet)(object)ConvertToInteger(s);
                else if (_type == typeof(double) || _type == typeof(double?)) _importFunc = s => (TRet)(object)ConvertToDouble(s);
                else if (_type == typeof(DateTime) || _type == typeof(DateTime?)) _importFunc = s => (TRet)(object)ConvertToDateTime(s);
                else if (_type == typeof(Boolean) || _type == typeof(Boolean?)) _importFunc = s => (TRet)(object)ConvertToBoolean(s);
                else throw new InvalidOperationException("the column type '" + _type.FullName + "' is unknown");
            }

            try
            {
                _setValueFunc(item, _importFunc(valueString));
            }
            catch(Exception ex)
            {
                throw new ImportException(_header, ex);
            }
        }

        public override string ToString()
        {
            return _dummyColumn ? "DummyColumn" : ((_header ?? "")) + " [" + _type + "]";
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
        private static Action<T, TRet> GetValueSetter(PropertyInfo propertyInfo)
        {
            if (typeof (T) != propertyInfo.DeclaringType)
            {
                throw new ArgumentException();
            }

            ParameterExpression instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            ParameterExpression argument = Expression.Parameter(typeof (TRet), "a");
            MethodCallExpression setterCall = Expression.Call(
                instance,
                propertyInfo.GetSetMethod(),
                Expression.Convert(argument, propertyInfo.PropertyType));

            return Expression.Lambda<Action<T, TRet>>
                (
                    setterCall, instance, argument
                ).Compile();
        }
    }
}