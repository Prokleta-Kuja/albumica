using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using albumica.Configuration;
using albumica.Data;
using albumica.FaceRecognition.DeepStack;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace albumica.FaceRecognition
{
    public class DeepStackFaceRecognition : IFaceRecogniton
    {
        public string ProviderName => "deepstack";
        private readonly AiDeepStackOptions Opt;
        private readonly IHttpClientFactory HttpFactory;
        public DeepStackFaceRecognition(IOptions<AiOptions> AiOpt, IHttpClientFactory httpFactory)
        {
            Opt = AiOpt.Value.DeepStack ?? throw new ArgumentNullException(nameof(AiOptions.DeepStack));
            HttpFactory = httpFactory;
        }
        private MultipartFormDataContent GetDefaultContent()
        {
            var content = new MultipartFormDataContent();
            if (!string.IsNullOrWhiteSpace(Opt.AdminKey))
                content.Add(new StringContent(Opt.AdminKey), "api_key");

            return content;
        }
        private HttpClient GetClient()
        {
            var client = HttpFactory.CreateClient();
            client.BaseAddress = new Uri(Opt.Endpoint!);
            return client;
        }
        public async Task<bool> AddFace(Person person, string imagePath)
        {
            var fi = new FileInfo(imagePath);
            if (!fi.Exists)
                return false;


            if (!person.AiPersonId.HasValue)
                person.AiPersonId = Guid.NewGuid();

            var content = GetDefaultContent();
            content.Add(new StringContent(person.AiPersonId.Value.ToString()), "user_id");

            using var fs = fi.OpenRead();
            content.Add(new StreamContent(fs), "image1", fi.Name);

            var client = GetClient();
            var output = await client.PostAsync("v1/vision/face/register", content);

            var json = await output.Content.ReadAsStringAsync();
            // {'message': 'face added', 'success': True}
            // {'error': 'user id not specified', 'success': False}
            // {'error': 'No valid image file found', 'success': False}

            return true;
        }
        public async Task<List<Face>?> RecognizeFaces(string imagePath)
        {

            var fi = new FileInfo(imagePath);
            if (!fi.Exists)
                return null;

            var content = GetDefaultContent();
            content.Add(new StringContent((Opt.MinConfidencePercent / 100d).ToString()), "min_confidence");

            using var fs = fi.OpenRead();
            content.Add(new StreamContent(fs), "image", fi.Name);

            var client = GetClient();
            var output = await client.PostAsync("v1/vision/face/recognize", content);

            var json = await output.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<Response>(json);
            //{'success': True, 'predictions': [{'userid': 'Idris Elba', 'y_min': 154, 'x_min': 1615, 'x_max': 1983, 'confidence': 0.76965684, 'y_max': 682}]}

            var faces = new List<Face>(response.predictions.Length);
            foreach (var prediction in response.predictions)
            {
                var width = prediction.x_max - prediction.x_min;
                var height = prediction.y_max - prediction.y_min;

                var face = new Face
                {
                    FaceId = Guid.NewGuid(),
                    Confidence = prediction.confidence,
                    X = prediction.x_min,
                    Y = prediction.y_min,
                    W = width,
                    H = height,
                };

                if (Guid.TryParse(prediction.userid, out var aiPersonId))
                    face.AiPersonId = aiPersonId;

                faces.Add(face);
            }

            return faces;
        }
    }
}