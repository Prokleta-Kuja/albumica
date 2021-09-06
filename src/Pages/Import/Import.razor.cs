using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using albumica.Configuration;
using albumica.Data;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace albumica.Pages.Import
{
    public partial class Import
    {
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IOptions<AiOptions> Ai { get; set; } = null!;
        [Inject] private ILogger<Import> Logger { get; set; } = null!;
        [Inject] private ILogger<ImportModel> ModelLogger { get; set; } = null!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
        private Preview? Preview;
        private Location? Location;
        private Persons? Persons;
        private DateTime? Created;
        private readonly IImport _t = LocalizationFactory.Import();
        private HashSet<string> Hashes = new();
        private IEnumerator<FileInfo> ImportableFiles = null!;
        private bool Loading = true;
        private bool NothingToImport = false;
        private bool Importing = false;
        private bool IsDuplicate = false;

        private ImportModel? Current;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var dir = new DirectoryInfo(C.Settings.ImportRootPath);
                ImportableFiles = dir.EnumerateFiles("*").GetEnumerator();

                var imageHashes = await Db.Images.Select(i => i.Hash).ToListAsync();
                foreach (var imageHash in imageHashes)
                    Hashes.Add(imageHash);

                Loading = false;

                await Next();
            }
        }
        private async Task Next()
        {
            Importing = IsDuplicate = false;
            Loading = true;
            StateHasChanged();

            if (ImportableFiles.MoveNext())
            {
                using var metaScope = ModelLogger.BeginScope("Loading metadata for {@file}", Current);
                Current = new ImportModel(ImportableFiles.Current, ModelLogger);

                await Current.ComputeHashAsync();
                if (Hashes.Contains(Current.Hash))
                    IsDuplicate = true;

                var metaDataLoaded = await Current.LoadAsync();
                if (!metaDataLoaded)
                {
                    Logger.LogError("Unknown media");
                    await Next();
                    return;
                }

                Created = Current.Created;
                Preview?.Change(Current);
                Location?.Change(Current);
                Persons?.Change(Current);
            }
            else
                NothingToImport = true;

            Loading = false;
            StateHasChanged();
        }
        private async Task Delete()
        {
            Current?.FileInfo.Delete();
            await Next();
        }
        private async Task ImportMedia()
        {
            if (Current == null || !Created.HasValue)
            {
                Logger.LogWarning("No date selected, cannot import");
                return;
            }

            Importing = true;
            StateHasChanged();

            var imageDestination = string.Empty;
            Video? video = null;

            if (!Current.IsVideo)
            {
                imageDestination = C.Settings.ImagesForPath(Created.Value, Current.FileInfo.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(imageDestination)!);
                Current.FileInfo.MoveTo(imageDestination, true);
            }
            else
            {
                imageDestination = C.Settings.ImagesForPath(Created.Value, Current.FileInfo.Name.Replace(Current.FileInfo.Extension, ".webp"));
                Directory.CreateDirectory(Path.GetDirectoryName(imageDestination)!);
                await Current.CreateVideoPreview(imageDestination);

                var videoDestination = C.Settings.VideosForPath(Created.Value, Current.FileInfo.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(videoDestination)!);
                Current.FileInfo.MoveTo(videoDestination);

                video = new Video(Path.GetRelativePath(C.Settings.VideosRootPath, videoDestination));
            }

            using var transaction = Db.Database.BeginTransaction();
            var img = new Image(Path.GetRelativePath(C.Settings.ImagesRootPath, imageDestination),
                                Created.Value,
                                Current.Hash);
            img.Video = video;

            if (Location != null && (Location.CurrentLocation.HasData || Location.CurrentLocation.HasGpsData))
                img.Location = Location.CurrentLocation;

            if (Persons != null && Persons.Selected.Any())
                img.Persons = Persons.Selected.Select(p => new ImagePerson { PersonId = p.Key, ForTraining = Persons.ForTraining })
                                              .ToList();

            Db.Images.Add(img);
            await Db.SaveChangesAsync();

            transaction.Commit();

            if (!Hashes.Contains(Current.Hash))
                Hashes.Add(Current.Hash);

            await Next();
        }
    }
}