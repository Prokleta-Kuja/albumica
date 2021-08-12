using System;

namespace albumica.FaceRecognition
{
    public class Face
    {
        public Guid FaceId { get; set; }
        public Guid? AiPersonId { get; set; }
        public float Confidence { get; set; }
        public float ConfidencePercent => Confidence * 100;
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }
}