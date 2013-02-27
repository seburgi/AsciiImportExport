#region using directives

using System;
using System.Globalization;
using System.Linq.Expressions;

#endregion

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
        private string _booleanFalse = "F";
        private string _booleanTrue = "T";
        private int _columnWidth = -1;
        private Func<TRet> _defaultValue;
        private Func<T, TRet, string> _exportFunc;
        private readonly Expression<Func<T, TRet>> _expression;
        private string _header;
        private Func<string, TRet> _importFunc;
        private string _stringFormat;
        private IFormatProvider _provider = CultureInfo.InvariantCulture;
        private char? _fillChar;
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
        /// Builds an initialized and ready to use instance of <see cref="IDocumentColumn{T}"/>
        /// </summary>
        /// <returns></returns>
        public IDocumentColumn<T> Build()
        {
            return new DocumentColumn<T, TRet>(_expression, _header, _defaultValue, _columnWidth, _alignment, _stringFormat, _provider, _booleanTrue, _booleanFalse, _importFunc, _exportFunc, _fillChar, _throwOnColumnOverflow);
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
            _defaultValue = value;
            return this;
        }

        /// <summary>
        /// Sets a custom export function that converts a value of type TRet to a string
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetExportFunc(Func<T, TRet, string> exportFunc)
        {
            _exportFunc = exportFunc;
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
            _importFunc = importFunc;
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
        /// Sets the string format used when exporting numerical data (Default = "0.0000")
        /// </summary>
        public DocumentColumnBuilder<T, TRet> SetStringFormat(string stringFormat)
        {
            _stringFormat = stringFormat;
            return this;
        }

        public DocumentColumnBuilder<T, TRet> SetFillChar(char fillChar)
        {
            _fillChar = fillChar;
            return this;
        }

        public void ThrowOnOverflow()
        {
            _throwOnColumnOverflow = true;
        }
    }
}