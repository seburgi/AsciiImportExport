#region using directives

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace AsciiImportExport
{
    internal class DocumentColumn<T, TRet> : IDocumentColumn<T>
    {
        private readonly string _booleanFalse;
        private readonly string _booleanTrue;
        private readonly Func<TRet> _getDefaultValueFunc;
        private Func<T, TRet, string> _exportFunc;
        private readonly char? _fillChar;
        private readonly bool _throwOnColumnOverflow;
        private readonly Func<T, TRet> _getValueFunc;
        private readonly string _header;
        private Func<string, TRet> _importFunc;
        private readonly bool _isDummyColumn;
        private Action<T, TRet> _setValueFunc;
        private readonly string _stringFormat;
        private readonly IFormatProvider _provider;
        private readonly Type _type;
        private readonly PropertyInfo _propertyInfo;

        public DocumentColumn(Expression<Func<T, TRet>> expression, string header, Func<TRet> getDefaultValueFunc, int columnWidth, ColumnAlignment alignment, string stringFormat, IFormatProvider provider, string booleanTrue, string booleanFalse, Func<string, TRet> importFunc, Func<T, TRet, string> exportFunc, char? fillChar, bool throwOnColumnOverflow)
        {
            _header = header;
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

            _getValueFunc = expression.Compile();

            MemberExpression me = ReflectionHelper.GetMemberExpression(expression);
            if (me != null)
            {
                _header = _header ?? me.Member.Name;
                _propertyInfo = me.Member as PropertyInfo;
                _type = _propertyInfo.PropertyType;
            }
            else
            {
                _header = _header ?? "";
                _isDummyColumn = true;
            }
        }

        public ColumnAlignment Alignment { get; private set; }

        public int ColumnWidth { get; private set; }

        public string Header
        {
            get { return _header; }
        }

        public string Format(string value, int columnWidth)
        {
            int width = columnWidth >= 0 ? columnWidth : 0;

            return (Alignment == ColumnAlignment.Right ? value.PadLeft(width, _fillChar ?? ' ') : value.PadRight(width, _fillChar ?? ' '));
        }

        public void Parse(T item, string value)
        {
            if (_isDummyColumn) return;
            if (_importFunc == null)
            {
                _importFunc = s => (TRet) ServiceStackTextHelpers.GetParseFn<TRet>(_stringFormat, _booleanTrue, _provider)(s);
            }

            try
            {
                TRet v;

                if (String.IsNullOrEmpty(value))
                {
                    if(_getDefaultValueFunc == null) return;
                    
                    v = _getDefaultValueFunc();
                }
                else
                {
                    v = _importFunc(value);
                }

                if (_setValueFunc == null)
                {
                    var setMethod = _propertyInfo.GetSetMethod(true);
                    if (setMethod != null)
                        _setValueFunc = GetValueSetter(_propertyInfo, setMethod);
                }

                _setValueFunc(item, v);
            }
            catch (Exception ex)
            {
                throw new ImportException(_header, value, ex);
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
                _exportFunc = (x, ret) => ServiceStackTextHelpers.GetSerializeFunc<TRet>(_stringFormat, _booleanTrue, _booleanFalse, _provider)(ret);
            }

            try
            {
                var exportedValue = _exportFunc(item, (TRet) value);
                if(_throwOnColumnOverflow && exportedValue.Length > ColumnWidth)
                    throw new OverflowException(string.Format("Column data width ({0}) exceded maximum column width ({1})", exportedValue.Length, ColumnWidth));
                return exportedValue;
            }
            catch (Exception ex)
            {
                throw new ExportException(_header, item, ex);
            }
        }

        public override string ToString()
        {
            return _isDummyColumn ? "DummyColumn" : ((_header ?? "")) + " [" + _type + "]";
        }

        private static Action<T, TRet> GetValueSetter(PropertyInfo propertyInfo, MethodInfo setMethod)
        {
            ParameterExpression instance = Expression.Parameter(typeof(T), "i");
            ParameterExpression argument = Expression.Parameter(typeof (TRet), "a");
            MethodCallExpression setterCall = Expression.Call(
                instance,
                setMethod,
                Expression.Convert(argument, propertyInfo.PropertyType));

            return Expression.Lambda<Action<T, TRet>>
                (
                    setterCall, instance, argument
                ).Compile();
        }
    }
}