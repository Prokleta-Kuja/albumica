namespace albumica.Data
{
    public class ImageTag
    {
        public int ImageId { get; set; }
        public int TagId { get; set; }

        public Image? Image { get; set; }
        public Tag? Tag { get; set; }
    }
}