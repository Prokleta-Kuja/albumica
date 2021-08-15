using System;
using System.IO;
using System.Threading.Tasks;
using albumica.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace albumica.Middlewares
{
    public static class ImageResizerExtensions
    {
        public static IApplicationBuilder UseImageResizer(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<ImageResizerMiddleware>();
        }
        public static IApplicationBuilder UseImageResizer(this IApplicationBuilder app, string matchUrlPrefix, string imageRootDir, string? cacheRootDir = null)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseImageResizer(new(matchUrlPrefix, imageRootDir, cacheRootDir));
        }
        public static IApplicationBuilder UseImageResizer(this IApplicationBuilder app, ImageResizerOptions options)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return app.UseMiddleware<ImageResizerMiddleware>(Options.Create(options));
        }
    }
    public class ImageResizerMiddleware
    {
        private readonly PathString _matchUrl;
        private readonly string _imagesRootDir;
        private readonly string _cacheRootDir;
        private readonly RequestDelegate _next;
        private readonly IContentTypeProvider _contentTypeProvider;

        public ImageResizerMiddleware(RequestDelegate next, IOptions<ImageResizerOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _contentTypeProvider = new FileExtensionContentTypeProvider();

            _matchUrl = options.Value.MathUrlPrefix;
            _imagesRootDir = options.Value.ImageRootDir;
            _cacheRootDir = options.Value.CacheRootDir;
        }

        /// <summary>
        /// Processes a request to determine if it matches a known file, and if so, serves it.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            if (!ValidateNoEndpoint(context))
            {
                //_logger.EndpointMatched();
            }
            else if (!ValidateMethod(context))
            {
                //_logger.RequestMethodNotSupported(context.Request.Method);
            }
            else if (!ValidatePath(context, _matchUrl, out var subPath))
            {
                //_logger.PathMismatch(subPath);
            }
            else if (!LookupContentType(_contentTypeProvider, subPath, out var contentType))
            {
                //_logger.FileTypeNotSupported(subPath);
            }
            else
            {
                // If we get here, we can try to serve the file
                return TryServeStaticFile(context, contentType, subPath);
            }

            return _next(context);
        }

        // Return true because we only want to run if there is no endpoint.
        private static bool ValidateNoEndpoint(HttpContext context) => context.GetEndpoint() == null;

        private static bool ValidateMethod(HttpContext context) => context.Request.Method == HttpMethods.Get || context.Request.Method == HttpMethods.Head;

        internal static bool ValidatePath(HttpContext context, PathString matchUrl, out PathString subPath) => TryMatchPath(context, matchUrl, forDirectory: false, out subPath);

        internal static bool LookupContentType(IContentTypeProvider contentTypeProvider, PathString subPath, out string? contentType)
        {
            return contentTypeProvider.TryGetContentType(subPath.Value!, out contentType);
        }
        internal static bool PathEndsInSlash(PathString path)
        {
            return path.HasValue && path.Value!.EndsWith("/", StringComparison.Ordinal);
        }
        internal static bool TryMatchPath(HttpContext context, PathString matchUrl, bool forDirectory, out PathString subpath)
        {
            var path = context.Request.Path;

            if (forDirectory && !PathEndsInSlash(path))
                path += new PathString("/");

            return path.StartsWithSegments(matchUrl, out subpath);
        }
        private async Task TryServeStaticFile(HttpContext context, string? contentType, PathString subPath)
        {
            if (!subPath.HasValue || string.IsNullOrWhiteSpace(subPath.Value))// = "/20170416_113320.jpg"
            {
                await _next(context);
                return;
            }

            var imgSubPath = subPath.Value.TrimStart('/');
            var originalPath = Path.Combine(_imagesRootDir, imgSubPath);
            var originalFI = new FileInfo(originalPath);

            if (!originalFI.Exists)
            {
                await _next(context);
                return;
            }

            FileInfo cacheFI;
            if (context.Request.Query.TryGetValue("w", out var w))
            {
                // TODO: Ensure this can't throw
                var width = Convert.ToInt32(w);
                var height = context.Request.Query.TryGetValue("h", out var h) ? Convert.ToInt32(h) : width;
                var mode = context.Request.Query.TryGetValue("m", out var m) ? Enum.Parse<ResizeMode>(m) : ResizeMode.Max;

                var fileName = Path.GetFileNameWithoutExtension(imgSubPath);
                var fileExt = Path.GetExtension(imgSubPath);
                var cacheFullName = $"{fileName}_{width}_{height}_{mode}{fileExt}";

                var cachePath = Path.Combine(_cacheRootDir, cacheFullName);
                cacheFI = new FileInfo(cachePath);
                if (!cacheFI.Exists)
                {
                    var img = Image.Load(originalFI.FullName);
                    var viewport = new ResizeOptions
                    {
                        Size = new Size(width, height),
                        Mode = mode,
                    };
                    img.Mutate(x => x.AutoOrient().Resize(viewport));
                    await img.SaveAsync(cacheFI.FullName);
                    cacheFI.Refresh();
                }
            }
            else
            {
                cacheFI = originalFI;
            }

            DateTimeOffset last = cacheFI.LastWriteTimeUtc;
            // Truncate to the second.
            var lastModified = new DateTimeOffset(last.Year, last.Month, last.Day, last.Hour, last.Minute, last.Second, last.Offset).ToUniversalTime();
            var length = cacheFI.Length;
            var etagHash = lastModified.ToFileTime() ^ length;
            var etagHeader = new EntityTagHeaderValue('\"' + Convert.ToString(etagHash, 16) + '\"');

            var requestHeaders = context.Request.GetTypedHeaders();
            var now = DateTimeOffset.UtcNow;

            // 14.24 If-Match
            var ifMatch = requestHeaders.IfMatch;
            if (ifMatch?.Count > 0)
            {
                var preconditionFailed = true;
                foreach (var etag in ifMatch)
                {
                    if (etag.Equals(EntityTagHeaderValue.Any) || etag.Compare(etagHeader, useStrongComparison: true))
                    {
                        preconditionFailed = false;
                        break;
                    }
                }
                if (preconditionFailed)
                {
                    context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                    return;
                }
            }

            // 14.28 If-Unmodified-Since
            var ifUnmodifiedSince = requestHeaders.IfUnmodifiedSince;
            if (ifUnmodifiedSince.HasValue && ifUnmodifiedSince <= now && ifUnmodifiedSince < lastModified)
            {
                context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                return;
            }

            if (!string.IsNullOrWhiteSpace(contentType))
                context.Response.ContentType = contentType;

            var responseHeaders = context.Response.GetTypedHeaders();
            responseHeaders.LastModified = lastModified;
            responseHeaders.ETag = etagHeader;
            responseHeaders.Headers.Add(HeaderNames.AcceptRanges, "bytes");

            // 14.25 If-Modified-Since
            var ifModifiedSince = requestHeaders.IfModifiedSince;
            if (ifModifiedSince.HasValue && ifModifiedSince <= now && ifModifiedSince >= lastModified)
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }

            responseHeaders.CacheControl = new() { NoCache = true };
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentLength = length;

            if (context.Request.Method == HttpMethods.Head)
                return;
            else
                try
                {
                    await context.Response.SendFileAsync(cacheFI.FullName, 0, length, context.RequestAborted);
                }
                catch (OperationCanceledException)
                {
                    // Don't throw this exception, it's most likely caused by the client disconnecting.
                }
        }

    }
}