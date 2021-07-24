using System;
using System.Collections.Generic;

namespace albumica.Data
{
    public class Image
    {
        public Image(string name, string extension, string relativePath, DateTime created, string hash)
        {
            Name = name;
            Extension = extension;
            RelativePath = relativePath;
            Created = created;
            Hash = hash;
        }

        public int ImageId { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string RelativePath { get; set; }
        public DateTime Created { get; set; }
        public string Hash { get; set; }
        public int? VideoId { get; set; }

        public ICollection<ImagePerson>? Persons { get; set; }
        public Video? Video { get; set; }
    }
}