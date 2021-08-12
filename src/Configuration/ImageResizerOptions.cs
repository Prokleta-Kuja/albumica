using System.IO;

namespace albumica.Configuration
{
    public class ImageResizerOptions
    {
        public ImageResizerOptions()
        {
            MathUrlPrefix = null!;
            ImageRootDir = null!;
            CacheRootDir = null!;
        }
        public ImageResizerOptions(string mathUrlPrefix, string imageRootDir, string? cacheRootDir = null)
        {
            MathUrlPrefix = mathUrlPrefix;
            ImageRootDir = imageRootDir;
            CacheRootDir = cacheRootDir ?? Path.GetTempPath();
        }

        public string MathUrlPrefix { get; set; }
        public string ImageRootDir { get; set; }
        public string CacheRootDir { get; set; }
    }
}