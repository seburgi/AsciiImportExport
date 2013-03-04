using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace AsciiImportExport
{
    /// <summary>
    /// Helps to build a column for a <see cref="DocumentFormatDefinition{T}"/>.
    /// Best used together with <see cref="DocumentFormatDefinitionBuilder{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the POCO you want to import/export</typeparam>
    /// <typeparam name="TRet">The type of the columns data</typeparam>
    public class DocumentColumnBuilder<T, TRet>
    {
        private ColumnAlignment _alignment = ColumnAlignment.Left;
        private string _booleanFalse = GlobalConfig.DefaultBooleanFalseString;
        private string _booleanTrue = GlobalConfig.DefaultBooleanTrueString;
        private int _columnWidth = -1;
        private Func<object> _defaultValue;
        private Func<T, object, string> _exportFunc;
        private readonly Expression<Func<T, TRet>> _expression;
        private char _fillChar = ' ';
        private string _header;
        private Func<string, object> _importFunc;
        private PropertyInfo _propertyInfo;
        private IFormatProvider _provider = GlobalConfig.DefaultFormatProvider;
        private string _stringFormat;
        private bool _throwOnColumnOverflow;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column</param>
        public DocumentColumnBuilder(Expression<Func<T, TRet>> expression)
        {
            _expression = expression;
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="propertyInfo">The property's PropertyInfo you want to import/export with this column</param>
        public DocumentColumnBuilder(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        /// <summary>
        /// Builds an initialized and ready to use instance of <see cref="IDocumentColumn{T}"/>
        /// </summary>
        /// <returns></returns>
        public IDocumentColumn<T> Build()
        {
            if (_expression != null)
            {
                MemberExpression me = Helpers.GetMemberExpression(_expression);
                if (me != null)
                {
                    _propertyInfo = me.Member as PropertyInfo;
                }
            }

            if (String.IsNullOrEmpty(_stringFormat))
            {
                Type columnType = _propertyInfo == null ? typeof (TRet) : _propertyInfo.PropertyType;

                if (columnType == typeof (DateTime) || columnType == typeof (DateTime?))
                    _stringFormat = GlobalConfig.DefaultDatetimeStringFormat;
                else
                    _stringFormat = GlobalConfig.DefaultNumericStringFormat;
            }

            return new DocumentColumn<T>(_propertyInfo, _header, _defaultValue, _columnWidth, _alignment, _stringFormat, _provider, _booleanTrue, _booleanFalse, _importFunc, _exportFunc, _fillChar, _throwOnColumnOverflow);
        }

        /// <summary>
        /// Sets the alignment of the data in the column (Default = Left)
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetAlignment(ColumnAlignment alignment)
        {
            _alignment = alignment;
            return this;
        }

        /// <summary>
        /// Sets the string representations for boolean True and False (Default = T, F)
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetBooleanStrings(string trueString, string falseString)
        {
            _booleanTrue = trueString;
            _booleanFalse = falseString;
            return this;
        }

        /// <summary>
        /// Sets a fixed width for the column (Default = -1 ... auto size)
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetColumnWidth(int columnWidth)
        {
            _columnWidth = columnWidth;
            return this;
        }

        /// <summary>
        /// Sets the default value of the column (Default = default(TRet))
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetDefaultValue(Func<TRet> value)
        {
            _defaultValue = Convert(value);
            return this;
        }

        /// <summary>
        /// Sets a custom export function that converts a value of type TRet to a string
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetExportFunc(Func<T, TRet, string> exportFunc)
        {
            _exportFunc = Convert(exportFunc);
            return this;
        }

        /// <summary>
        /// Sets the fill character to use for a column (Default = ' ')
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetFillChar(char fillChar)
        {
            _fillChar = fillChar;
            return this;
        }

        /// <summary>
        /// Sets the format information of the column
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetFormatProvider(IFormatProvider provider)
        {
            _provider = provider;
            return this;
        }

        /// <summary>
        /// Sets the header of the column (Default = name of property)
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetHeader(string header)
        {
            _header = header;
            return this;
        }

        /// <summary>
        /// Sets a custom import function that converts a string to a value of type TRet
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetImportFunc(Func<string, TRet> importFunc)
        {
            _importFunc = Convert(importFunc);
            return this;
        }

        /// <summary>
        /// Sets the string format used when exporting numerical data (Default = "0.0000")
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetStringFormat(string stringFormat)
        {
            _stringFormat = stringFormat;
            return this;
        }

        public void ThrowOnOverflow()
        {
            _throwOnColumnOverflow = true;
        }

        private Func<object> Convert(Func<TRet> myActionT)
        {
            if (myActionT == null) return null;

            return () => myActionT();
        }

        private Func<T, object, string> Convert(Func<T, TRet, string> myActionT)
        {
            if (myActionT == null) return null;

            return (arg, o) => myActionT(arg, (TRet) o);
        }

        private Func<string, object> Convert(Func<string, TRet> myActionT)
        {
            if (myActionT == null) return null;

            return s => myActionT(s);
        }
    }
}