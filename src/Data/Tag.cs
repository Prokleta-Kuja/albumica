using System.Collections.Generic;

namespace albumica.Data
{
    public class Tag
    {
        public Tag(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public int TagId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public ICollection<ImageTag>? Images { get; set; }
    }
}