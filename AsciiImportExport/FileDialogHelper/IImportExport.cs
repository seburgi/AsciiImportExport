namespace AsciiImportExport.FileDialogHelper
{
    public interface IImportExport
    {
        string FileExtension { get; }
        string FileTypeFilter { get; }
        string FileTypeName { get; }
        string Name { get; }
    }
}