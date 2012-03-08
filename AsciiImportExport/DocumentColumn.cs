#region using directives

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace AsciiImportExport
{
    public class DocumentColumn<T, TRet> : IDocumentColumn<T> where T : class, new()
    {
        private readonly string _booleanFalse;
        private readonly string _booleanTrue;
        private readonly string _dateTimeStringFormat;
        private readonly object _defaultValue;
        private readonly string _doubleStringFormat;
        private readonly bool _dummyColumn;
        private Func<TRet, string> _exportFunc;
        private readonly Func<T, TRet> _getValueFunc;
        private readonly string _header;
        private Func<string, TRet> _importFunc;
        private readonly Action<T, TRet> _setValueFunc;
        private readonly Type _type;

        public DocumentColumn(Expression<Func<T, TRet>> expression, string header, TRet defaultValue, int columnWidth, ColumnAlignment alignment, string doubleStringFormat, string dateTimeStringFormat, string booleanTrue, string booleanFalse, Func<string, TRet> importFunc, Func<TRet, string> exportFunc)
        {
            _header = header;
            _defaultValue = defaultValue;
            _doubleStringFormat = doubleStringFormat;
            ColumnWidth = columnWidth;
            Alignment = alignment;
            _dateTimeStringFormat = dateTimeStringFormat;
            _booleanTrue = booleanTrue;
            _booleanFalse = booleanFalse;
            _importFunc = importFunc;
            _exportFunc = exportFunc;
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

        public ColumnAlignment Alignment { get; private set; }

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

        public void SetValue(T item, string valueString)
        {
            if (_dummyColumn) return;
            if (valueString == null) return;

            if (_importFunc == null)
            {
                if (_type == typeof (String)) _importFunc = s => (TRet) (object) s;
                else if (_type == typeof (int) || _type == typeof (int?)) _importFunc = s => (TRet) (object) ConvertToInteger(s);
                else if (_type == typeof (double) || _type == typeof (double?)) _importFunc = s => (TRet) (object) ConvertToDouble(s);
                else if (_type == typeof (DateTime) || _type == typeof (DateTime?)) _importFunc = s => (TRet) (object) ConvertToDateTime(s);
                else if (_type == typeof (Boolean) || _type == typeof (Boolean?)) _importFunc = s => (TRet) (object) ConvertToBoolean(s);
                else throw new InvalidOperationException("the column type '" + _type.FullName + "' is unknown");
            }

            try
            {
                _setValueFunc(item, _importFunc(valueString));
            }
            catch (Exception ex)
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
            return value == null ? DateTime.MinValue : DateTime.ParseExact(value, _dateTimeStringFormat, CultureInfo.InvariantCulture.DateTimeFormat);
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
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString(_dateTimeStringFormat, CultureInfo.InvariantCulture.DateTimeFormat));
        }

        private string FormatAsDouble(double? value)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString(_doubleStringFormat, CultureInfo.InvariantCulture.NumberFormat));
        }

        private string FormatAsInteger(int? value)
        {
            return FormatAsString(!value.HasValue ? "" : value.Value.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        private string FormatAsString(string str)
        {
            int width = ColumnWidth >= 0 ? ColumnWidth : 0;

            return (Alignment == ColumnAlignment.Right ? str.PadLeft(width) : str.PadRight(width));
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