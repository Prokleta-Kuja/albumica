using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using albumica.Data;
using albumica.FaceRecognition;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace albumica.Pages.Import
{
    public partial class Persons
    {
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IFaceRecogniton FaceRecognition { get; set; } = null!;
        private readonly IImport _t = LocalizationFactory.Import();
        private readonly Formats _f = LocalizationFactory.Formats();
        private List<PersonTagModel> Tags = new();
        private HashSet<string> Selected = new();
        private bool ForTraining = false;
        private bool AiEnabled = false;
        private List<Face>? Faces;
        private Dictionary<Rectangle, string> FaceImages = new();
        private ResizeOptions PreviewResize = new()
        {
            Size = new Size(100, 100),
            Mode = ResizeMode.Pad,
        };
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            AiEnabled = FaceRecognition.ProviderName != "none";
            Tags = await Db.People.Select(p => new PersonTagModel(p, p.Images!.Count)).ToListAsync();
            Tags.Sort();
            StateHasChanged();
        }

        public async Task ChangeImage(ImportImageModel model)
        {
            ForTraining = false;
            Selected.Clear();

            if (!AiEnabled)
            {
                StateHasChanged();
                return;
            }

            FaceImages.Clear();
            StateHasChanged();
            Faces = await FaceRecognition.RecognizeFaces(model.FullName);

            if (Faces == null)
            {
                //TODO: display error and remove loading
            }
            else
            {
                var img = SixLabors.ImageSharp.Image.Load(model.FullName);
                var fileName = Path.GetFileNameWithoutExtension(model.Uri);
                var fileExt = Path.GetExtension(model.Uri);

                foreach (var face in Faces)
                {
                    // DeepStack sometimes has negative values??
                    if (face.X < 0 || face.Y < 0 || face.W < 0 || face.H < 0)
                        continue;

                    // TODO: Make sure inside bounds x+w < img.Width, etc

                    var rect = new Rectangle(face.X, face.Y, face.W, face.H);
                    if (FaceImages.ContainsKey(rect))
                        continue;

                    var imgText = img.Clone(x =>
                    {
                        x.Crop(rect);
                        x.AutoOrient();
                        x.Resize(PreviewResize);
                    }).ToBase64String(PngFormat.Instance);
                    FaceImages.Add(rect, imgText);
                }
            }

            StateHasChanged();
        }
        public void OnTag(PersonTagModel model)
        {
            if (Selected.Contains(model.Name))
            {
                Selected.Remove(model.Name);
                model.ImageCount--;
            }
            else
            {
                Selected.Add(model.Name);
                model.ImageCount++;
            }

            ForTraining = Selected.Count == 1;
        }
    }
}