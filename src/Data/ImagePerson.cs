using System;

namespace albumica.Data
{
    public class ImagePerson
    {
        public int PersonId { get; set; }
        public int ImageId { get; set; }
        public bool ForTraining { get; set; }
        public Guid? FaceId { get; set; } // Not null if trained

        public Person? Person { get; set; }
        public Image? Image { get; set; }
    }
}