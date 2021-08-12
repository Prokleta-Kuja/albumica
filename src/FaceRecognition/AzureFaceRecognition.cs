using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using albumica.Configuration;
using albumica.Data;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace albumica.FaceRecognition
{
    public class AzureFaceRecognition : IFaceRecogniton
    {
        public string ProviderName => "azure";
        private readonly AiAzureOptions Opt;
        private readonly IFaceClient Client;
        private string? PersonGroupId;
        private readonly ResizeOptions AzureResizeOptions;
        public AzureFaceRecognition(IOptions<AiOptions> AiOpt)
        {
            Opt = AiOpt.Value.Azure ?? throw new ArgumentNullException(nameof(AiOptions.Azure));

            var creds = new ApiKeyServiceClientCredentials(Opt.Key);
            Client = new FaceClient(creds) { Endpoint = Opt.Endpoint };

            // Resize for Azure long edge not greater then 1920
            AzureResizeOptions = new ResizeOptions
            {
                Size = new Size(1920, 1920),
                Mode = ResizeMode.Max,
            };

            var azureFile = new FileInfo(C.Settings.DataPath(ProviderName));
            if (azureFile.Exists)
                PersonGroupId = File.ReadAllText(azureFile.FullName);
        }
        private async Task<string> GetPersonGroupId()
        {
            if (string.IsNullOrWhiteSpace(PersonGroupId))
            {
                PersonGroupId = Guid.NewGuid().ToString();
                await Client.PersonGroup.CreateAsync(PersonGroupId, nameof(albumica), recognitionModel: Opt.RecognitionModel);
                File.WriteAllText(C.Settings.DataPath(ProviderName), PersonGroupId);
            }

            return PersonGroupId!;
        }
        public async Task<bool> AddFace(Person person, string imagePath)
        {
            var personGroupId = await GetPersonGroupId();

            await Task.CompletedTask;
            return false;
        }
        public async Task<List<Face>?> RecognizeFaces(string imagePath)
        {
            var personGroupId = await GetPersonGroupId();

            using var imgStream = new MemoryStream();
            using var img = await SixLabors.ImageSharp.Image.LoadAsync(imagePath);
            if (img == null)
                return null;

            var originalWidth = img.Width;
            var originalHeight = img.Height;

            img.Mutate(x => x.Resize(AzureResizeOptions));
            // TODO: remove all exif info except orientation
            img.Metadata.ExifProfile = null;
            img.Save(imgStream, JpegFormat.Instance);

            var ratioWidth = (decimal)originalWidth / (decimal)img.Width;
            var ratioHeight = (decimal)originalHeight / (decimal)img.Height;

            imgStream.Seek(0, SeekOrigin.Begin);
            var predictions = await Client.Face.DetectWithStreamAsync(imgStream, recognitionModel: Opt.RecognitionModel, detectionModel: Opt.DetectionModel);

            var faces = new List<Face>(predictions.Count);
            foreach (var prediction in predictions)
            {
                var face = new Face
                {
                    FaceId = prediction.FaceId ?? Guid.NewGuid(),
                    Confidence = 0,
                    X = Convert.ToInt32(prediction.FaceRectangle.Left * ratioHeight),
                    Y = Convert.ToInt32(prediction.FaceRectangle.Top * ratioWidth),
                    W = Convert.ToInt32(prediction.FaceRectangle.Width * ratioWidth),
                    H = Convert.ToInt32(prediction.FaceRectangle.Height * ratioHeight),
                };

                // TODO: this are just faces, to identify call Identify Async
                // if (Guid.TryParse(prediction.userid, out var aiPersonId))
                //     face.AiPersonId = aiPersonId;

                faces.Add(face);
            }

            return faces;
        }
    }
}