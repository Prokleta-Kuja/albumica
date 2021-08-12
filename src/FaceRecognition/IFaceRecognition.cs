using System.Collections.Generic;
using System.Threading.Tasks;
using albumica.Data;

namespace albumica.FaceRecognition
{
    public interface IFaceRecogniton
    {
        string ProviderName { get; }
        Task<bool> AddFace(Person person, string imagePath);
        Task<List<Face>?> RecognizeFaces(string imagePath); // Null means error
    }
}