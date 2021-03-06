using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using albumica.Models;
using albumica.Translations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using SixLabors.ImageSharp.Processing;

namespace albumica.Pages.Import
{
    public partial class Preview : IDisposable
    {
        const string MEDIA_IMPORT = nameof(MEDIA_IMPORT);
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
        private readonly IImport _t = LocalizationFactory.Import();
        private DotNetObjectReference<Preview>? ThisRef;
        private IJSObjectReference? ElementSizeRef;
        private string CurrentImageOriginalUri = string.Empty;
        private string CurrentImageUri = string.Empty;
        private string CurrentVideoUri = string.Empty;
        private bool Loading;
        private int Width;
        private int Height;
        private int ViewPortWidth;
        private double DevicePixelRatio = 1;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ThisRef = DotNetObjectReference.Create(this);
                ElementSizeRef = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/MediaContainerSize.js");

                await ElementSizeRef.InvokeVoidAsync("initialize", ThisRef, MEDIA_IMPORT);
            }
        }

        public void Dispose()
        {
            if (ThisRef != null)
                ThisRef.Dispose();
            if (ElementSizeRef != null)
                ElementSizeRef.DisposeAsync().GetAwaiter();
        }
        public void Change(ImportModel model)
        {
            Loading = true;
            if (model.IsVideo)
            {
                CurrentImageOriginalUri = CurrentImageUri = string.Empty;
                CurrentVideoUri = model.Uri;
                ChangeVideoUri();
            }
            else
            {
                CurrentVideoUri = string.Empty;
                CurrentImageOriginalUri = model.Uri;
                ChangeImageUri();
            }
            Loading = false;
        }
        private void ChangeImageUri()
        {
            if (string.IsNullOrWhiteSpace(CurrentImageOriginalUri) || Width == 0 || Height == 0)
                return;

            Loading = true;
            StateHasChanged();

            var w = (int)(Width * DevicePixelRatio);
            var h = (int)(Height * DevicePixelRatio);
            var mode = ViewPortWidth < 768 ? ResizeMode.Min : ResizeMode.Max;

            var qs = new Dictionary<string, string?>{
                { "w", w.ToString() },
            };

            if (mode == ResizeMode.Max)
                qs.Add("h", h.ToString());
            else
                qs.Add("m", mode.ToString());

            CurrentImageUri = QueryHelpers.AddQueryString(CurrentImageOriginalUri, qs);
            Loading = false;
            StateHasChanged();
        }
        private void ChangeVideoUri()
        {
            if (string.IsNullOrWhiteSpace(CurrentVideoUri) || Width == 0 || Height == 0)
                return;

            Loading = true;
            StateHasChanged();

            Loading = false;
            StateHasChanged();
        }

        [JSInvokable]
        public void ElementChanged(int width, int height, int viewPortWidth, double devicePixelRatio = 1)
        {
            Width = width;
            Height = height;
            ViewPortWidth = viewPortWidth;
            DevicePixelRatio = devicePixelRatio;

            ChangeImageUri();
            ChangeVideoUri();
        }
    }
}