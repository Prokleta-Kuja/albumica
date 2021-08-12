namespace albumica.Configuration
{
    public class AiOptions
    {
        public const string Section = "Ai";
        public AiAzureOptions? Azure { get; set; }
        public AiDeepStackOptions? DeepStack { get; set; }
    }
    public class AiAzureOptions
    {
        public string? Key { get; set; }
        public string? Endpoint { get; set; }
        public string RecognitionModel { get; set; } = Microsoft.Azure.CognitiveServices.Vision.Face.Models.RecognitionModel.Recognition04;
        public string DetectionModel { get; set; } = Microsoft.Azure.CognitiveServices.Vision.Face.Models.DetectionModel.Detection03;
    }
    public class AiDeepStackOptions
    {
        public string? AdminKey { get; set; }
        public string? Endpoint { get; set; }
        public int MinConfidencePercent { get; set; } = 67;
    }
}