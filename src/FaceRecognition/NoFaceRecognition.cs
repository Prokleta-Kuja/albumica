using System.Collections.Generic;
using System.Threading.Tasks;
using albumica.Data;

namespace albumica.FaceRecognition
{
    public class NoFaceRecognition : IFaceRecogniton
    {
        public string ProviderName => "none";

        public Task<bool> AddFace(Person person, string imagePath)
        {
            return Task.FromResult(false);
        }

        public Task<List<Face>?> RecognizeFaces(string imagePath)
        {
            return Task.FromResult<List<Face>?>(null);
        }
    }
}