using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using albumica.Configuration;
using albumica.Data;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace albumica.Pages.Import
{
    public partial class Import
    {
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IOptions<AiOptions> Ai { get; set; } = null!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
        private Preview? Preview;
        private Location? Location;
        private Persons? Persons;
        private readonly IImport _t = LocalizationFactory.Import();
        private IEnumerator<FileInfo> ImportableFiles = null!;

        private ImportImageModel? Current;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var dir = new DirectoryInfo(C.Settings.ImportRootPath);
                ImportableFiles = dir.EnumerateFiles("*").GetEnumerator();

                await Skip();
            }
        }
        private async Task Skip()
        {
            if (ImportableFiles.MoveNext())
            {
                Current = new ImportImageModel(ImportableFiles.Current);

                Preview?.ChangeImage(Current);
                Location?.ChangeImage(Current);
                Persons?.ChangeImage(Current);

                await Task.CompletedTask;
            }
            else
            {
                // TODO: Handle nothing to import
            }
        }
        private async Task ImportImage()
        {
            // TODO: handle import
            await Skip();
        }
    }
}