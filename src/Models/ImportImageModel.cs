using System.IO;

namespace albumica.Models
{
    public class ImportImageModel
    {
        public ImportImageModel(FileInfo file)
        {
            FullName = file.FullName;

            var relativePath = Path.GetRelativePath(C.Settings.ImportRootPath, FullName);
            Uri = C.Routes.ResizerImportFor(relativePath);
        }

        public string FullName { get; set; }
        public string Uri { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }
}