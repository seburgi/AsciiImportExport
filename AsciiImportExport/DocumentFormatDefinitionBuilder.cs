using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AsciiImportExport
{
    /// <summary>
    /// Helps to build a <see cref="DocumentFormatDefinition{T}"/>. Call Build() at the end of the method-chain.
    /// </summary>
    /// <typeparam name="T">The type of the POCO you want to import/export.</typeparam>
    public class DocumentFormatDefinitionBuilder<T>
    {
        private readonly bool _autosizeColumns;
        private readonly string _columnSeparator;
        private readonly List<IDocumentColumn<T>> _columns = new List<IDocumentColumn<T>>();
        private string _commentString;
        private bool _exportHeaderLine;
        private string _headerLinePraefix;
        private Func<T> _instantiator;
        private bool _lineEndsWithColumnSeparator;
        private bool _trimLineEnds = true;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="columnSeparator">String that is used to separate columns in the text</param>
        /// <param name="autoSizeColumns">Defines if the rows of a column shall all be of the same width</param>
        public DocumentFormatDefinitionBuilder(string columnSeparator, bool autoSizeColumns)
        {
            _columnSeparator = columnSeparator;
            _autosizeColumns = autoSizeColumns;

            ServiceStackTextHelpers.EmptyCtorDelegate defaultInitializer = ServiceStackTextHelpers.GetConstructorMethodToCache(typeof (T));
            _instantiator = () => (T) defaultInitializer();
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
        /// Adds a new column
        /// </summary>
        /// <typeparam name="TRet">The type of the columns data</typeparam>
        /// <param name="expression">Lambda expression that evaluates the property of the POCO you want to import/export with this column</param>
        /// <param name="stringFormat">String format of the property</param>
        /// <param name="customization">Action that enables additionial customizations of the <see cref="DocumentColumnBuilder{T,TRet}"/></param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddColumn<TRet>(Expression<Func<T, TRet>> expression, string stringFormat, Action<DocumentColumnBuilder<T, TRet>> customization = null)
        {
            DocumentColumnBuilder<T, TRet> builder = new DocumentColumnBuilder<T, TRet>(expression).SetStringFormat(stringFormat);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="propertyInfo">The property's PropertyInfo you want to import/export with this column</param>
        /// <param name="stringFormat">String format of the property</param>
        /// <param name="customization">Action that enables additionial customizations of the <see cref="DocumentColumnBuilder{T,TRet}"/></param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddColumn(PropertyInfo propertyInfo, Action<DocumentColumnBuilder<T, object>> customization = null)
        {
            var builder = new DocumentColumnBuilder<T, object>(propertyInfo);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        /// <summary>
        /// Adds a new column
        /// </summary>
        /// <param name="propertyInfo">The property's PropertyInfo you want to import/export with this column</param>
        /// <param name="stringFormat">String format of the property</param>
        /// <param name="customization">Action that enables additionial customizations of the <see cref="DocumentColumnBuilder{T,TRet}"/></param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddColumn(PropertyInfo propertyInfo, string stringFormat, Action<DocumentColumnBuilder<T, object>> customization = null)
        {
            DocumentColumnBuilder<T, object> builder = new DocumentColumnBuilder<T, object>(propertyInfo).SetStringFormat(stringFormat);

            if (customization != null)
                customization(builder);

            _columns.Add(builder.Build());

            return this;
        }

        /// <summary>
        /// Adds a column to the definition that acts as a place holder. Useful when you don't care about a column when importing data.
        /// </summary>
        /// <param name="exportValue">The value that will be exported</param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddDummyColumn(string exportValue)
        {
            return AddColumn<string>(null, builder => builder.SetExportFunc((item, x) => exportValue).SetColumnWidth(exportValue.Length));
        }

        /// <summary>
        /// Adds a column to the definition that acts as a place holder. Useful when you don't care about a column when importing data.
        /// </summary>
        /// <param name="columnWidth">The width of the dummy column</param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> AddDummyColumn(int columnWidth)
        {
            return AddColumn<string>(null, builder => builder.SetExportFunc((item, x) => "").SetColumnWidth(columnWidth));
        }

        /// <summary>
        /// Automatically builds a ready to use instance of <see cref="DocumentFormatDefinition{T}"/> with columns for all public propertys of the provided type/>
        /// </summary>
        /// <param name="columnSeparator">String that is used to separate columns in the text</param>
        /// <param name="autoSizeColumns">Defines if the rows of a column shall all be of the same width</param>
        /// <param name="commentString">String that is used to identify the start of comments in the text (Default = No comments)</param>
        /// <param name="exportHeaderLine">Defines if a header line shall be created during serialization (Default = false)</param>
        /// <param name="headerLinePraefix">Optional praefix for the header line (Default = empty string)</param>
        /// <returns></returns>
        public static IDocumentFormatDefinition<T> AutoBuild(string columnSeparator, bool autoSizeColumns, string commentString, bool exportHeaderLine, string headerLinePraefix = "")
        {
            List<PropertyInfo> propertyList = typeof (T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            return Build(propertyList, columnSeparator, autoSizeColumns, commentString, exportHeaderLine, headerLinePraefix);
        }

        /// <summary>
        /// Builds an initialized and ready to use instance of <see cref="DocumentFormatDefinition{T}"/>
        /// </summary>
        /// <returns></returns>
        public IDocumentFormatDefinition<T> Build()
        {
            return new DocumentFormatDefinition<T>(_columns, _columnSeparator, _commentString, _autosizeColumns, _exportHeaderLine, _headerLinePraefix, _instantiator, _lineEndsWithColumnSeparator, _trimLineEnds);
        }

        /// <summary>
        /// Builds a ready to use instance of <see cref="DocumentFormatDefinition{T}"/> with columns for all properties in the provided property list />
        /// </summary>
        /// <param name="propertyList">List of properties to import / export</param>
        /// <param name="columnSeparator">String that is used to separate columns in the text</param>
        /// <param name="autoSizeColumns">Defines if the rows of a column shall all be of the same width</param>
        /// <param name="commentString">String that is used to identify the start of comments in the text (Default = No comments)</param>
        /// <param name="exportHeaderLine">Defines if a header line shall be created during serialization (Default = false)</param>
        /// <param name="headerLinePraefix">Optional praefix for the header line (Default = empty string)</param>
        /// <returns></returns>
        public static IDocumentFormatDefinition<T> Build(List<PropertyInfo> propertyList, string columnSeparator, bool autoSizeColumns, string commentString, bool exportHeaderLine, string headerLinePraefix = "")
        {
            DocumentFormatDefinitionBuilder<T> builder = new DocumentFormatDefinitionBuilder<T>(columnSeparator, autoSizeColumns)
                .SetCommentString(commentString)
                .SetExportHeaderLine(exportHeaderLine, headerLinePraefix);

            foreach (var property in propertyList)
            {
                builder.AddColumn(property);
            }

            return builder.Build();
        }

        /// <summary>
        /// String that is used to identify the start of comments in the text (Default = No comments)
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetCommentString(string commentString)
        {
            _commentString = commentString;
            return this;
        }

        /// <summary>
        /// Defines if a header line shall be created during serialization (Default = false)
        /// </summary>
        /// <param name="exportHeaderLine">Defines if a header line shall be created during serialization (Default = false)</param>
        /// <param name="headerLinePraefix">Optional praefix for the header line (Default = empty string)</param>
        /// <returns></returns>
        public DocumentFormatDefinitionBuilder<T> SetExportHeaderLine(bool exportHeaderLine, string headerLinePraefix = "")
        {
            _exportHeaderLine = exportHeaderLine;
            _headerLinePraefix = headerLinePraefix;
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

        /// <summary>
        /// Defines if during export each line shall be terminated with the column separator. (Default = false)
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetLineEndsWithColumnSeparator(bool lineEndsWithColumnSeparator)
        {
            _lineEndsWithColumnSeparator = lineEndsWithColumnSeparator;
            return this;
        }

        /// <summary>
        /// Defines if line endings get trimmed during export. (Default = true)
        /// </summary>
        public DocumentFormatDefinitionBuilder<T> SetTrimLineEnds(bool trimLineEnds)
        {
            _trimLineEnds = trimLineEnds;
            return this;
        }
    }
}