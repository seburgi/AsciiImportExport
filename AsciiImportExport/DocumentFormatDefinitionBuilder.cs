#region using directives

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    /// Helps to build a <see cref="DocumentFormatDefinition{T}"/>. Call Build() at the end of the method-chain.
    /// </summary>
    /// <typeparam name="T">The type of the POCO you want to import/export.</typeparam>
    public class DocumentFormatDefinitionBuilder<T> where T : new()
    {
        private bool _autosizeColumns;
        private string _columnSeparator = ";";
        private readonly List<IDocumentColumn<T>> _columns = new List<IDocumentColumn<T>>();
        private string _commentString = "'";
        private bool _exportHeaderLine;
        private Func<T> _instantiator = () => new T();

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column</param>
        /// <param name="doubleStringFormat">Sets the string format used when exporting numerical data</param>
        /// <param name="customization">Action that enables additionial customizations of the <see cref="DocumentColumnBuilder{T,TRet}"/></param>
        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, double?>> expression, string doubleStringFormat, Action<DocumentColumnBuilder<T, double?>> customization = null)
        {
            DocumentColumnBuilder<T, double?> builder = new DocumentColumnBuilder<T, double?>(expression).SetDoubleStringFormat(doubleStringFormat);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column</param>
        /// <param name="dateTimeStringFormat">The string format used when importing/exporting DateTimes</param>
        /// <param name="customization">Action that enables additionial customizations of the <see cref="DocumentColumnBuilder{T,TRet}"/></param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, DateTime?>> expression, string dateTimeStringFormat, Action<DocumentColumnBuilder<T, DateTime?>> customization = null)
        {
            DocumentColumnBuilder<T, DateTime?> builder = new DocumentColumnBuilder<T, DateTime?>(expression).SetDateTimeStringFormat(dateTimeStringFormat);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column</param>
        /// <param name="trueString">This string is used to represent the boolean value 'True'</param>
        /// <param name="falseString">This string is used to represent the boolean value 'False'</param>
        /// <param name="customization">Action that enables additionial customizations of the <see cref="DocumentColumnBuilder{T,TRet}"/></param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddColumn(Expression<Func<T, bool?>> expression, string trueString, string falseString, Action<DocumentColumnBuilder<T, bool?>> customization = null)
        {
            DocumentColumnBuilder<T, bool?> builder = new DocumentColumnBuilder<T, bool?>(expression).SetBooleanStrings(trueString, falseString);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="column">The column</param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddColumn(IDocumentColumn<T> column)
        {
            _columns.Add(column);
            return this;
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <typeparam name="TRet">The type of the columns data</typeparam>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column</param>
        /// <param name="customization">Action that enables additionial customizations of the <see cref="DocumentColumnBuilder{T,TRet}"/></param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddColumn<TRet>(Expression<Func<T, TRet>> expression, Action<DocumentColumnBuilder<T, TRet>> customization = null)
        {
            var builder = new DocumentColumnBuilder<T, TRet>(expression);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        /// <summary>
        /// Adds a column to the definition that acts as a place holder. Useful when you don't care about a column when importing data.
        /// </summary>
        /// <param name="exportValue">The value that will be exported (Default = empty string)</param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddDummyColumn(string exportValue = null, int columnWidth = -1)
        {
            return AddColumn(x => "", builder => builder.SetExportFunc(x => exportValue).SetColumnWidth(columnWidth));
        }

        /// <summary>
        /// Builds an initialized and ready to use instance of <see cref="DocumentFormatDefinition{T}"/>
        /// </summary>
        /// <returns></returns>
        public DocumentFormatDefinition<T> Build()
        {
            return new DocumentFormatDefinition<T>(_columns, _columnSeparator, _commentString, _autosizeColumns, _exportHeaderLine, _instantiator);
        }

        /// <summary>
        /// Defines if the rows of a column shall all be of the same width (Default = False)
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetAutosizeColumns(bool autosizeColumns)
        {
            _autosizeColumns = autosizeColumns;
            return this;
        }

        /// <summary>
        /// String that is used to separate columns in the text (Default = ;)
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetColumnSeparator(string columnSeparator)
        {
            _columnSeparator = columnSeparator;
            return this;
        }

        /// <summary>
        /// String that is used to identify the start of comments in the text (Default = ')
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetCommentString(string commentString)
        {
            _commentString = commentString;
            return this;
        }

        /// <summary>
        /// Defines if a header line shall be created during serialization (Default = False)
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetExportHeaderLine(bool exportHeaderLine)
        {
            _exportHeaderLine = exportHeaderLine;
            return this;
        }

        /// <summary>
        /// Function that creates a new instance of type <see cref="T"/>.
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetInstantiator(Func<T> func)
        {
            _instantiator = func;
            return this;
        }
    }
}