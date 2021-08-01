using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace albumica.Pages.Import
{
    public partial class Preview : IDisposable
    {
        const string IMAGE_IMPORT = nameof(IMAGE_IMPORT);
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
        [Parameter] public ImportImageModel? Model { get; set; }
        private readonly IImport _t = LocalizationFactory.Import();
        private DotNetObjectReference<Preview>? ThisRef;
        private IJSObjectReference? ElementSizeRef;
        private string CurrentImageUri = string.Empty;

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
            if (Model == null || width == 0 || height == 0)
                return;

            var w = (int)(width * devicePixelRatio);
            var h = (int)(height * devicePixelRatio);

            var qs = new Dictionary<string, string?>{
                { "w", w.ToString() },
                { "h", h.ToString() },
            };

            CurrentImageUri = QueryHelpers.AddQueryString(Model.Uri, qs);
            StateHasChanged();
        }
    }
}