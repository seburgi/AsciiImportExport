namespace AsciiImportExport.FileDialogHelper
{
    public static class ImportExportHelper
    {
        public static string GetFileTypeFilter(bool includeAllFiles, params IImportExport[] imExporterArray)
        {
            string allFileTypeFilter = "";
            string fileTypeFilter = "";
            foreach (var imExporter in imExporterArray)
            {
                allFileTypeFilter += ";*" + imExporter.FileExtension;
                fileTypeFilter += "|" + imExporter.FileTypeFilter;
            }


            string result = null;
            if (includeAllFiles)
            {
                allFileTypeFilter = allFileTypeFilter.Substring(1);
                result = "All Formats (" + allFileTypeFilter + ")|" + allFileTypeFilter + fileTypeFilter;
            }
            else
            {
                result = fileTypeFilter.Substring(1);
            }

            return result;
        }
    }
}