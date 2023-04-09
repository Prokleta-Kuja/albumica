using System.IO.Compression;
using System.Text;
using albumica.Entities;
using albumica.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace albumica.Controllers;

[ApiController]
[Route("api/media")]
[Tags(nameof(Media))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class MediaController : ControllerBase
{
    static readonly FileExtensionContentTypeProvider s_ctp = new();
    readonly ILogger<MediaController> _logger;
    readonly AppDbContext _db;
    public MediaController(ILogger<MediaController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet(Name = "GetMedia")]
    [ProducesResponseType(typeof(ListResponse<MediaLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] FilterQuery req)
    {
        var query = _db.Media.AsNoTracking();
        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<MediaSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                MediaSortBy.Created => query.Order(m => m.Created, req.Ascending),
                MediaSortBy.IsVideo => query.Order(m => m.IsVideo, req.Ascending),
                _ => query
            };
        else
        {
            req.Ascending = false;
            req.SortBy = nameof(MediaSortBy.Created);
            query = query.Order(m => m.Created, req.Ascending);
        }

        var items = await query
            .Paginate(req)
            .Select(m => new MediaLM(m.Original, m.Preview)
            {
                Id = m.MediaId,
                IsVideo = m.IsVideo,
                Created = m.Created,
            })
            .ToListAsync();

        return Ok(new ListResponse<MediaLM>(req, count, items));
    }

    [HttpGet("{mediaId:int}", Name = "GetMediaById")]
    [ProducesResponseType(typeof(MediaVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int mediaId)
    {
        var media = await _db.Media
           .AsNoTracking()
           .Where(m => m.MediaId == mediaId)
           .Select(m => new MediaVM(m))
           .FirstOrDefaultAsync();

        if (media == null)
            return NotFound(new PlainError("Not found"));

        return Ok(media);
    }

    [HttpDelete("{mediaId:int}", Name = "DeleteMedia")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int mediaId)
    {
        var media = await _db.Media
          .Where(m => m.MediaId == mediaId)
          .FirstOrDefaultAsync();

        if (media == null)
            return NotFound(new PlainError("Not found"));

        using var transaction = _db.Database.BeginTransaction();
        _db.Media.Remove(media);
        await _db.SaveChangesAsync();
        System.IO.File.Delete(C.Paths.MediaDataFor(media.Original));
        if (!string.IsNullOrWhiteSpace(media.Preview))
            System.IO.File.Delete(C.Paths.MediaDataFor(media.Preview));

        await transaction.CommitAsync();

        return NoContent();
    }
    [HttpGet("{*path}")]
    public IActionResult GetFile(string path)
    {
        var filePath = C.Paths.MediaDataFor(path);
        var contentType = GetResponseContentTypeOrDefault(filePath);

        return PhysicalFile(filePath, contentType);
    }
    [AllowAnonymous]
    [HttpGet("zip/{bundleId:int}")]
    public async Task GetZip(int bundleId)
    {
        //Response.ContentLength = file.Length; ??????
        // Response.ContentType = s_ctp.Mappings[".zip"];
        // Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{bundleId}.zip\"");
        // using var zip = new ZipArchive(Response.BodyWriter.AsStream(), ZipArchiveMode.Create);
        // foreach (var item in items)
        // {
        // var entry = zip.CreateEntry(file.Name);
        // entry.ExternalAttributes = entry.ExternalAttributes | (Convert.ToInt32("664", 8) << 16);
        // using var entryStream = entry.Open();
        // using var fileStream = file.OpenRead();
        // await fileStream.CopyToAsync(entryStream);
        // }
        await Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("Nope"));
    }
    static string GetResponseContentTypeOrDefault(string path)
        => s_ctp.TryGetContentType(path, out var matchedContentType) ? matchedContentType : "application/octet-stream";
}

public enum MediaSortBy
{
    Created = 0,
    IsVideo = 1,
}