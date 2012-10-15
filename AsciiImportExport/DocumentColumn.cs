#region using directives

using System;
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
        private readonly Func<T, TRet> _getValueFunc;
        private readonly string _header;
        private Func<string, TRet> _importFunc;
        private readonly bool _isDummyColumn;
        private readonly Action<T, TRet> _setValueFunc;
        private readonly string _stringFormat;
        private readonly Type _type;

        public DocumentColumn(Expression<Func<T, TRet>> expression, string header, Func<TRet> getDefaultValueFunc, int columnWidth, ColumnAlignment alignment, string stringFormat, string booleanTrue, string booleanFalse, Func<string, TRet> importFunc, Func<T, TRet, string> exportFunc)
        {
            _header = header;
            _getDefaultValueFunc = getDefaultValueFunc;
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

                var setMethod = propertyInfo.GetSetMethod();
                if(setMethod != null)
                    _setValueFunc = GetValueSetter(propertyInfo, setMethod);
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

            return (Alignment == ColumnAlignment.Right ? value.PadLeft(width) : value.PadRight(width));
        }

        public void Parse(T item, string value)
        {
            if (_isDummyColumn) return;
            if (_importFunc == null)
            {
                _importFunc = s => (TRet) ServiceStackTextHelpers<TRet>.GetParseFn(_stringFormat, _booleanTrue)(s);
            }
            
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
            
            try
            {
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
                //_exportFunc = (item, ret) => ServiceStackTextHelpers<TRet>.GetSerializeFunc(_stringFormat, _booleanTrue, _booleanFalse)(ret);
                _exportFunc = (x, ret) => ServiceStackTextHelpers<TRet>.GetSerializeFunc(_stringFormat, _booleanTrue, _booleanFalse)(ret);
            }

            try
            {
                return _exportFunc(item, (TRet)value);
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
            if (typeof (T) != propertyInfo.DeclaringType)
            {
                throw new ArgumentException();
            }

            ParameterExpression instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
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