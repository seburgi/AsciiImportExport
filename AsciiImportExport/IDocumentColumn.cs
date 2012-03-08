namespace AsciiImportExport
{
    public interface IDocumentColumn<in T>
    {
        ColumnAlignment Alignment { get; }
        int ColumnWidth { get; }
        string FormattedHeader { get; }
        string Format(T item);
        void SetValue(T item, string valueString);
    }
}