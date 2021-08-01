using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using albumica.Data;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace albumica.Pages.Import
{
    public partial class Import
    {
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
        private readonly IImport _t = LocalizationFactory.Import();
        private IEnumerator<FileInfo> ImportableFiles = null!;

        private ImportImageModel? Current;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var dir = new DirectoryInfo(C.Settings.ImportRootPath);
                ImportableFiles = dir.EnumerateFiles("*").GetEnumerator();

                if (ImportableFiles.MoveNext())
                {
                    Current = new ImportImageModel(ImportableFiles.Current);

                    StateHasChanged();
                }
                else
                {
                    // TODO: Handle nothing to import
                    await Task.CompletedTask;
                }
            }
        }


    }
}