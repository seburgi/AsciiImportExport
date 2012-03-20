#region using directives

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    /// Defines a column in a document
    /// </summary>
    /// <typeparam name="T">The type of the POCO you want to import/export</typeparam>
    /// <typeparam name="TRet">The type of the columns data</typeparam>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column.</param>
        /// <param name="header">Header of the column. Will be set to the name of the property if left null.</param>
        /// <param name="defaultValue">The default value of the column.</param>
        /// <param name="columnWidth">The width of the column. Set -1 for auto width</param>
        /// <param name="alignment">The alignment of the data in the column.</param>
        /// <param name="doubleStringFormat">This is the string format used when exporting numerical data.</param>
        /// <param name="dateTimeStringFormat">This is the string format used when importing/exporting DateTime.</param>
        /// <param name="booleanTrue">This string is used to represent the boolean value 'True'</param>
        /// <param name="booleanFalse">This string is used to represent the boolean value 'False'</param>
        /// <param name="importFunc">Custom import function that converts a string to a value of type TRet</param>
        /// <param name="exportFunc">Custom import function that converts a value of type TRet to a string</param>
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

        /// <summary>
        /// The alignment of the data in the column
        /// </summary>
        public ColumnAlignment Alignment { get; private set; }

        /// <summary>
        /// The fixed width of the column (-1 if not fixed)
        /// </summary>
        public int ColumnWidth { get; private set; }

        /// <summary>
        /// The formatted header of the column (correctly padded and aligned)
        /// </summary>
        public string FormattedHeader
        {
            get
            {
                string header = Serialize(_header);
                return header;
            }
        }

        /// <summary>
        /// The header of the column
        /// </summary>
        public string Header
        {
            get { return _header; }
        }

        /// <summary>
        /// Parses the input string to a value of type TRet and assigns the value to the property of the item object
        /// </summary>
        /// <param name="item">the item that the parsed value will be assigned to</param>
        /// <param name="value">the input string</param>
        public void Parse(T item, string value)
        {
            if (_dummyColumn) return;
            if (value == null) return;

            if (_importFunc == null)
            {
                if (_type == typeof (String)) _importFunc = s => (TRet) (object) s;
                else if (_type == typeof (int) || _type == typeof (int?)) _importFunc = s => (TRet) (object) ParseInteger(s);
                else if (_type == typeof (double) || _type == typeof (double?)) _importFunc = s => (TRet) (object) ParseDouble(s);
                else if (_type == typeof (DateTime) || _type == typeof (DateTime?)) _importFunc = s => (TRet) (object) ParseDateTime(s);
                else if (_type == typeof (Boolean) || _type == typeof (Boolean?)) _importFunc = s => (TRet) (object) ParseToBoolean(s);
                else throw new InvalidOperationException("the column type '" + _type.FullName + "' is unknown");
            }

            try
            {
                _setValueFunc(item, _importFunc(value));
            }
            catch (Exception ex)
            {
                throw new ImportException(_header, ex);
            }
        }

        /// <summary>
        /// Serializes to string
        /// </summary>
        /// <param name="item">the item holding the data to be serialized</param>
        /// <returns></returns>
        public string Serialize(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            object columnValue = _getValueFunc.Invoke(item);

            if (columnValue == null && _defaultValue != null) columnValue = _defaultValue;

            if (_exportFunc == null)
            {
                if (_type == typeof (String)) _exportFunc = value => Serialize(value as string);
                else if (_type == typeof (Int32) || _type == typeof (Int32?)) _exportFunc = value => Serialize(value as int?);
                else if (_type == typeof (Double) || _type == typeof (Double?)) _exportFunc = value => Serialize(value as double?);
                else if (_type == typeof (Boolean) || _type == typeof (Boolean?)) _exportFunc = value => Serialize(value as Boolean?);
                else if (_type == typeof (DateTime) || _type == typeof (DateTime?)) _exportFunc = value => Serialize(value as DateTime?);
                else throw new InvalidOperationException("the column type '" + _type.FullName + "' is unknown");
            }

            return _exportFunc((TRet) columnValue);
        }

        /// <summary>
        /// ToString override for easier debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _dummyColumn ? "DummyColumn" : ((_header ?? "")) + " [" + _type + "]";
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

        /// <summary>
        /// Parses the input string to a boolean
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns></returns>
        private bool ParseToBoolean(string value)
        {
            if (value == _booleanTrue) return true;
            if (value == _booleanFalse) return false;

            if (_defaultValue != null) return (bool) _defaultValue;

            throw new InvalidOperationException("invalid value '" + value + "' in boolean column '" + _header + "'");
        }

        /// <summary>
        /// Parses the input string to a DateTime
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns></returns>
        private DateTime? ParseDateTime(string value)
        {
            return value == null ? DateTime.MinValue : DateTime.ParseExact(value, _dateTimeStringFormat, CultureInfo.InvariantCulture.DateTimeFormat);
        }

        /// <summary>
        /// Parses the input string to a double
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns></returns>
        private double? ParseDouble(string value)
        {
            return String.IsNullOrEmpty(value) ? (double?) _defaultValue : Double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Parses the input string to an <see cref="Int32"/>
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns></returns>
        private int? ParseInteger(string value)
        {
            return String.IsNullOrEmpty(value) ? (int?) _defaultValue : Int32.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Serializes the input value to a string
        /// </summary>
        /// <param name="value">The input value bool?</param>
        /// <returns></returns>
        private string Serialize(bool? value)
        {
            return Serialize(!value.HasValue ? "" : (value.Value ? _booleanTrue : _booleanFalse));
        }

        /// <summary>
        /// Serializes the input value to a string
        /// </summary>
        /// <param name="value">The input value of type DateTime?</param>
        /// <returns></returns>
        private string Serialize(DateTime? value)
        {
            return Serialize(!value.HasValue ? "" : value.Value.ToString(_dateTimeStringFormat, CultureInfo.InvariantCulture.DateTimeFormat));
        }

        /// <summary>
        /// Serializes the input value to a string
        /// </summary>
        /// <param name="value">The input value of type double?</param>
        /// <returns></returns>
        private string Serialize(double? value)
        {
            return Serialize(!value.HasValue ? "" : value.Value.ToString(_doubleStringFormat, CultureInfo.InvariantCulture.NumberFormat));
        }

        /// <summary>
        /// Serializes the input value to a string
        /// </summary>
        /// <param name="value">The input value of type int?</param>
        /// <returns></returns>
        private string Serialize(int? value)
        {
            return Serialize(!value.HasValue ? "" : value.Value.ToString(CultureInfo.InvariantCulture.NumberFormat));
        }

        /// <summary>
        /// Serializes the input value to a string
        /// </summary>
        /// <param name="value">The input value of type string</param>
        /// <returns></returns>
        private string Serialize(string str)
        {
            int width = ColumnWidth >= 0 ? ColumnWidth : 0;

            return (Alignment == ColumnAlignment.Right ? str.PadLeft(width) : str.PadRight(width));
        }
    }
}