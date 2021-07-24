using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace albumica
{
    public static class TestFace
    {
        public static async Task Test()
        {
            var key1 = "";
            var endpoint = "";

            var recognitionModel = RecognitionModel.Recognition04;
            var detectionModel = DetectionModel.Detection03;

            var client = new FaceClient(new ApiKeyServiceClientCredentials(key1))
            {
                Endpoint = endpoint
            };

            // Create a dictionary for all your images, grouping similar ones under the same key.
            var personGroupId = "943a6e97-1476-423c-bcdf-67f09c09ef8c";
            //var personGroupName = "Obitelj";
            var personDictionary = new Dictionary<string, string[]>
            {
                // { "Nela", "n1,n2,n3,n4,n5,n6,n8,n9,n10".Split(",") }, // n7 too large
                // { "Vita", "v1,v2,v3".Split(",") },
            };

            // Create a person group. 
            //Console.WriteLine($"Create a person group ({personGroupName}).");
            //await client.PersonGroup.CreateAsync(personGroupId, personGroupName, recognitionModel: recognitionModel);

            // The similar faces will be grouped into a single person group person.
            foreach (var personKey in personDictionary.Keys)
            {
                // Limit TPS
                await Task.Delay(2500);
                var person = await client.PersonGroupPerson.CreateAsync(personGroupId, personKey);

                Console.WriteLine($"Create a person group person '{personKey}'.");

                // Add face to the person group person.
                foreach (var personImage in personDictionary[personKey])
                {
                    Console.WriteLine($"Add face to the person group person({personKey}) from image `{personImage}`");

                    using var imageStream = File.OpenRead($"slike/train/{personImage}.jpg");
                    var face = await client.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, person.PersonId, imageStream, personImage);
                    // var face2 = await client.PersonGroupPerson.AddFaceFromUrlAsync(personGroupId, person.PersonId, $"{url}{personImage}", personImage);
                }
            }

            // // Start to train the person group.
            // Console.WriteLine();
            // Console.WriteLine($"Train person group {personGroupName}.");
            // await client.PersonGroup.TrainAsync(personGroupId);

            // // Wait until the training is completed.
            // while (true)
            // {
            //     await Task.Delay(10000);
            //     var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
            //     Console.WriteLine($"Training status: {trainingStatus.Status}.");
            //     if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
            // }
            // Console.WriteLine();

            //---------------------------------------------
            // imagesharp
            // Resize and rotate
            using var img = await Image.LoadAsync("slike/vt3.jpg");
            var opt = new ResizeOptions
            {
                Size = new Size(1920, 1920), // 1920x1080 max for face api
                Mode = ResizeMode.Max,
            };
            img.Mutate(x =>
            {
                x.Resize(opt);
                //x.BlackWhite();
            });

            // The library automatically picks an encoder based on the file extension then
            // encodes and write the data to disk.
            // You can optionally set the encoder to choose.
            img.Save("slike/vt3_bw.jpg");

            //-------------------------------------------------

            using var detectImageStream = File.OpenRead("slike/vt3_bw.jpg");

            // Detect faces from source image url.
            //var detectedFaces = await DetectFaceRecognize(client, $"{url}{sourceImageFileName}", recognitionModel);
            var detectedFaces = await client.Face.DetectWithStreamAsync(detectImageStream, recognitionModel: recognitionModel, detectionModel: detectionModel);
            Console.WriteLine($"{detectedFaces.Count} face(s) detected");

            // Add detected faceId to sourceFaceIds.
            var detectedFaceIds = new List<Guid>();
            if (detectedFaceIds != null)
                foreach (var detectedFace in detectedFaces)
                    if (detectedFace.FaceId.HasValue)
                        detectedFaceIds.Add(detectedFace.FaceId.Value);

            // Identify the faces in a person group. 
            var identifyResults = await client.Face.IdentifyAsync(detectedFaceIds, personGroupId);

            foreach (var identifyResult in identifyResults)
            {
                if (identifyResult.Candidates.Any())
                {
                    // var person = await client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                    // Console.WriteLine($"Person '{person.Name}' is identified for face - {identifyResult.FaceId}," +
                    //     $" confidence: {identifyResult.Candidates[0].Confidence}.");
                    Console.WriteLine($"Face recognized - {identifyResult.Candidates[0].PersonId}");
                }
                else
                {
                    Console.WriteLine($"Face not recognized - {identifyResult.FaceId}");
                }
            }
            Console.WriteLine();
            // Vita f0570a17-902e-43dd-a101-79a14db46821
            // Nela 5d9289a1-bcdd-43c8-8997-8cbcd1a93817
        }
    }
}