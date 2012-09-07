#region using directives

#endregion

namespace AsciiImportExport.FileDialogHelper
{
    public abstract class ImportExportBase : IImportExport
    {
        public abstract string FileExtension { get; }

        public string FileTypeFilter
        {
            get { return FileTypeName + " (*" + FileExtension + ")|*" + FileExtension; }
        }

        public abstract string FileTypeName { get; }
        public abstract string Name { get; }
    }
}