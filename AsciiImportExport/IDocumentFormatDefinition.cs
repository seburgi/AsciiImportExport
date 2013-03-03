using System.Collections.Generic;
using System.IO;

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
        /// Serializes an enumerable list of the desired type to a string
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        string Export(IEnumerable<T> items);

        /// <summary>
        /// Deserializes a string to a list of the desired type
        /// </summary>
        /// <param name="reader">The text reader</param>
        /// <param name="skipLines">Tells the importer to skip a certain amount of lines at the beginning of a file.</param>
        /// <returns></returns>
        List<T> Import(TextReader reader, int skipLines = 0);

        /// <summary>
        /// Deserializes a string to a list of the desired type
        /// </summary>
        /// <param name="text">The text to import</param>
        /// <param name="skipLines">Tells the importer to skip a certain amount of lines at the beginning of a file.</param>
        /// <returns></returns>
        List<T> Import(string text, int skipLines = 0);
    }
}