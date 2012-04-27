#region using directives

using System;
using System.Collections.Generic;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    /// Holds all the format information necessary to import/export columnbased text data
    /// </summary>
    /// <typeparam name="T">The type of the POCO you want to import/export</typeparam>
    public interface IDocumentFormatDefinition<T>
    {
        /// <summary>
        /// String that is used to separate columns in the text
        /// </summary>
        string ColumnSeparator { get; }

        /// <summary>
        /// Enumerable list of columns of type <see cref="IDocumentColumn{T}"/> defining the structure of a document
        /// </summary>
        IEnumerable<IDocumentColumn<T>> Columns { get; }

        /// <summary>
        /// String that is used to identify the start of comments in the text
        /// </summary>
        string CommentString { get; }

        /// <summary>
        /// Serializes an enumerable list of type <see cref="T"/> to a string
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        string Export(IEnumerable<T> items);

        /// <summary>
        /// Parses a string to a generic List of type <see cref="T"/>
        /// </summary>
        /// <param name="fileContent">The input string</param>
        /// <returns></returns>
        List<T> Import(string fileContent);

        /// <summary>
        /// Parses an enumerable list of strings to a generic List of type <see cref="T"/>
        /// </summary>
        /// <param name="lines">The rows of a document</param>
        /// <returns></returns>
        List<T> Import(IEnumerable<string> lines);
    }
}