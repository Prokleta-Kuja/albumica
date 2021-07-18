using System.Collections.Generic;

namespace albumica.Data
{
    public class Tag
    {
        public Tag(string name)
        {
            Name = name;
        }
        public int TagId { get; set; }
        public string Name { get; set; }
        public TagCategory Category { get; set; }
        public string? Metadata { get; set; }

        public ICollection<ItemTag>? Items { get; set; }
    }
}