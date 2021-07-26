namespace albumica.Data
{
    public class Video
    {
        public Video(string name, string extension, string relativePath)
        {
            Name = name;
            Extension = extension;
            RelativePath = relativePath;
        }

        public int VideoId { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string RelativePath { get; set; }

        public Image? Image { get; set; }
    }
}