namespace albumica.Data
{
    public class Video
    {
        public Video(string relativePath)
        {
            RelativePath = relativePath;
        }

        public int VideoId { get; set; }
        public string RelativePath { get; set; }

        public Image? Image { get; set; }
    }
}