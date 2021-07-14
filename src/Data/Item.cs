using System;
using System.Collections.Generic;

namespace albumica.Data
{
    public class Item
    {
        public Item(string name, string extension, string path, DateTime created, string hash)
        {
            Name = name;
            Extension = extension;
            Path = path;
            Created = created;
            Hash = hash;
            Tags = new HashSet<ItemTag>();
        }

        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public DateTime Created { get; set; }
        public string Hash { get; set; }

        public ICollection<ItemTag>? Tags { get; set; }
    }
}