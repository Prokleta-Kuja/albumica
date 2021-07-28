using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using albumica.Data;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace albumica.Pages
{
    public partial class Import : IDisposable
    {
        const string IMAGE_IMPORT = nameof(IMAGE_IMPORT);
        [Inject] private AppDbContext Db { get; set; } = null!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
        private DotNetObjectReference<Import>? ThisRef;
        private IJSObjectReference? ElementSizeRef;
        private readonly IImport _t = LocalizationFactory.Import();

        private string CurrentImagePath = "img-import/IMG_20210714_073839.jpg";
        private string CurrentImageUri = "";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ThisRef = DotNetObjectReference.Create(this);
                ElementSizeRef = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/ImageContainerSize.js");

                await ElementSizeRef.InvokeVoidAsync("initialize", ThisRef, IMAGE_IMPORT);
            }
        }
        public void Dispose()
        {
            if (ThisRef != null)
                ThisRef.Dispose();
            if (ElementSizeRef != null)
                ElementSizeRef.DisposeAsync().GetAwaiter();
        }
        [JSInvokable]
        public void ElementChanged(double width, double height, double devicePixelRatio = 1)
        {
            if (width == 0 || height == 0)
                return;

            var w = (int)(width * devicePixelRatio);
            var h = (int)(height * devicePixelRatio);

            var qs = new Dictionary<string, string?>{
                { "w", w.ToString() },
                { "h", h.ToString() },
            };

            CurrentImageUri = QueryHelpers.AddQueryString(CurrentImagePath, qs);
            StateHasChanged();
        }
    }
}