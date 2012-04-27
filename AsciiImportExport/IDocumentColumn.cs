#region using directives

using System;

#endregion

namespace AsciiImportExport
{
    /// <summary>
    /// A column in a document
    /// </summary>
    /// <typeparam name="T">The type of the POCO you want to import/export</typeparam>
    public interface IDocumentColumn<in T>
    {
        /// <summary>
        /// The alignment of the data in the column
        /// </summary>
        ColumnAlignment Alignment { get; }

        /// <summary>
        /// The fixed width of the column (-1 if not fixed)
        /// </summary>
        int ColumnWidth { get; }

        /// <summary>
        /// The header of the column
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Returns the input string correctly padded to the specified length
        /// </summary>
        /// <param name="value">The input string</param>
        /// <param name="columnWidth">The targeted length of the serialized string</param>
        /// <returns></returns>
        string Format(string value, int columnWidth);

        /// <summary>
        /// Parses the input string to a value of type TRet and assigns the value to the property of the item object
        /// </summary>
        /// <param name="item">the item that the parsed value will be assigned to</param>
        /// <param name="value">the input string</param>
        void Parse(T item, string value);

        /// <summary>
        /// Serializes to string
        /// </summary>
        /// <param name="item">the item holding the data to be serialized</param>
        /// <returns></returns>
        string Serialize(T item);
    }
}