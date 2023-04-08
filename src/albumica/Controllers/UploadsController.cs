using albumica.Jobs;
using albumica.Models;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace albumica.Controllers;

[ApiController]
[Route("api/uploads")]
[Tags("Upload")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class UploadsController : ControllerBase
{
    readonly ILogger<UploadsController> _logger;
    readonly IBackgroundJobClient _job;
    public UploadsController(ILogger<UploadsController> logger, IBackgroundJobClient job)
    {
        _logger = logger;
        _job = job;
    }

    [HttpPost(Name = "UploadFiles")]
    //[RequestTimeout] TODO: will be available in .Net 8
    [RequestSizeLimit(5368709120)] // 5GB
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadLargeFile()
    {
        var request = HttpContext.Request;

        // validation of Content-Type
        // 1. first, it must be a form-data request
        // 2. a boundary should be found in the Content-Type
        if (!request.HasFormContentType ||
            !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
            string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            return new UnsupportedMediaTypeResult();

        var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body, 1024 * 1024 / 2);
        var section = await reader.ReadNextSectionAsync();

        var saved = 0;
        while (section != null)
        {
            if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition)
                && contentDisposition.DispositionType.Equals("form-data")
                && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
            {
                var saveToPath = C.Paths.QueueDataFor(contentDisposition.FileName.Value);
                if (!System.IO.File.Exists(saveToPath))
                    try
                    {
                        using (var targetStream = System.IO.File.Create(saveToPath))
                            await section.Body.CopyToAsync(targetStream);
                        saved++;
                    }
                    catch (Exception)
                    {
                        System.IO.File.Delete(saveToPath);
                        throw;
                    }
            }

            section = await reader.ReadNextSectionAsync();
        }

        if (saved > 0)
        {
            _logger.LogDebug("Uploaded {Count} media", saved);
            return Ok();
        }

        return BadRequest(new PlainError("No files data in the request."));
    }
    [HttpPatch(Name = "ProcessQueue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ProcessQueue()
    {
        _logger.LogDebug("Starting up processing queue job");
        _job.Enqueue<ProcessQueue>(j => j.Run(CancellationToken.None));

        return Ok();
    }
}