using System;
using System.Collections.Generic;

namespace albumica.Data
{
    public class Image
    {
        public Image(string relativePath, DateTime created, string hash)
        {
            RelativePath = relativePath;
            Created = created;
            Hash = hash;
        }

        public int ImageId { get; set; }
        public string RelativePath { get; set; }
        public DateTime Created { get; set; }
        public string Hash { get; set; }
        public int? VideoId { get; set; }
        public int? LocationId { get; set; }

        public ICollection<ImagePerson>? Persons { get; set; }
        public Video? Video { get; set; }
        public Location? Location { get; set; }
    }
}