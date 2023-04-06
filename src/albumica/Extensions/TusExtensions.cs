using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

namespace albumica.Extensions;

public static class TusExtensions
{
    // TODO: configure Kestrel and reverse proxy
    static readonly TusDiskStore _store = new(C.Paths.TempData);
    public static void AddTusEndpoint(this WebApplication app)
    {
        app.MapTus("/uploads", async httpCtx => new DefaultTusConfiguration
        {
            Store = _store,
            MaxAllowedUploadSizeInBytesLong = null, // Unlimited, adjust Kestrel accordingly
            UsePipelinesIfAvailable = true,
            Expiration = new AbsoluteExpiration(TimeSpan.FromHours(2)),
            MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
            Events = new()
            {
                OnFileCompleteAsync = async eventCtx =>
                {
                    var file = await eventCtx.GetFileAsync();
                    var metadata = await file.GetMetadataAsync(eventCtx.CancellationToken);
                    using var stream = await file.GetContentAsync(eventCtx.CancellationToken);
                    // TODO: move to queue
                    await stream.DisposeAsync();

                    if (eventCtx.Store is ITusTerminationStore terminationStore)
                        await terminationStore.DeleteFileAsync(file.Id, eventCtx.CancellationToken);
                }
            }
        }).RequireAuthorization();
    }
}