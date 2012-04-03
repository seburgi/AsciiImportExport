#region using directives

using System;
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
    public class DocumentColumn<T, TRet> : IDocumentColumn<T>
    {
        private readonly string _booleanFalse;
        private readonly string _booleanTrue;
        private readonly object _defaultValue;
        private Func<TRet, string> _exportFunc;
        private readonly Func<T, TRet> _getValueFunc;
        private readonly string _header;
        private Func<string, TRet> _importFunc;
        private readonly bool _isDummyColumn;
        private readonly Action<T, TRet> _setValueFunc;
        private readonly string _stringFormat;
        private readonly Type _type;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column.</param>
        /// <param name="header">Header of the column. Will be set to the name of the property if left null.</param>
        /// <param name="defaultValue">The default value of the column.</param>
        /// <param name="columnWidth">The width of the column. Set -1 for auto width</param>
        /// <param name="alignment">The alignment of the data in the column.</param>
        /// <param name="stringFormat">This is the string format used when exporting numerical data.</param>
        /// <param name="booleanTrue">This string is used to represent the boolean value 'True'</param>
        /// <param name="booleanFalse">This string is used to represent the boolean value 'False'</param>
        /// <param name="importFunc">Custom import function that converts a string to a value of type TRet</param>
        /// <param name="exportFunc">Custom export function that converts a value of type TRet to a string</param>
        public DocumentColumn(Expression<Func<T, TRet>> expression, string header, TRet defaultValue, int columnWidth, ColumnAlignment alignment, string stringFormat, string booleanTrue, string booleanFalse, Func<string, TRet> importFunc, Func<TRet, string> exportFunc)
        {
            _header = header;
            _defaultValue = defaultValue;
            _stringFormat = stringFormat;
            ColumnWidth = columnWidth;
            Alignment = alignment;
            _booleanTrue = booleanTrue;
            _booleanFalse = booleanFalse;
            _importFunc = importFunc;
            _exportFunc = exportFunc;

            _getValueFunc = expression.Compile();

            MemberExpression me = ReflectionHelper.GetMemberExpression(expression);
            if (me != null)
            {
                _header = _header ?? me.Member.Name;
                var propertyInfo = me.Member as PropertyInfo;
                _type = propertyInfo.PropertyType;
                _setValueFunc = GetValueSetter(propertyInfo);
            }
            else
            {
                _isDummyColumn = true;
            }
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
        /// The header of the column
        /// </summary>
        public string Header
        {
            get { return _header; }
        }

        /// <summary>
        /// Returns the input string correctly padded to the specified length
        /// </summary>
        /// <param name="value">The input string</param>
        /// <param name="columnWidth">The targeted length of the serialized string</param>
        public string Format(string value, int columnWidth)
        {
            int width = columnWidth >= 0 ? columnWidth : 0;

            return (Alignment == ColumnAlignment.Right ? value.PadLeft(width) : value.PadRight(width));
        }

        /// <summary>
        /// Parses the input string to a value of type TRet and assigns the value to the property of the item object
        /// </summary>
        /// <param name="item">the item that the parsed value will be assigned to</param>
        /// <param name="value">the input string</param>
        public void Parse(T item, string value)
        {
            if (_isDummyColumn) return;
            if (String.IsNullOrEmpty(value)) return;

            if (_importFunc == null)
            {
                _importFunc = s => (TRet) ServiceStackTextHelpers<TRet>.GetParseFn(_stringFormat, _booleanTrue)(s);
            }

            try
            {
                _setValueFunc(item, _importFunc(value));
            }
            catch (Exception ex)
            {
                throw new ImportException(_header, value, ex);
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

            object value = _getValueFunc.Invoke(item);

            if (value == null && _defaultValue != null) value = _defaultValue;

            if (_exportFunc == null)
            {
                _exportFunc = ret => ServiceStackTextHelpers<TRet>.GetSerializeFunc(_stringFormat, _booleanTrue, _booleanFalse)(ret);
            }

            return _exportFunc((TRet) value);
        }

        /// <summary>
        /// ToString override for easier debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _isDummyColumn ? "DummyColumn" : ((_header ?? "")) + " [" + _type + "]";
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