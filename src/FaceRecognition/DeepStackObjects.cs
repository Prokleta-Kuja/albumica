namespace albumica.FaceRecognition.DeepStack
{
    class Response
    {
        public bool success { get; set; }
        public Face[] predictions { get; set; } = null!;

    }

    class Face
    {
        public string userid { get; set; } = null!;
        public float confidence { get; set; }
        public int y_min { get; set; }
        public int x_min { get; set; }
        public int y_max { get; set; }
        public int x_max { get; set; }

    }
}