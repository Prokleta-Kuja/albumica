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
    public async Task<IActionResult> GetAllAsync([FromQuery] MediaQuery req)
    {
        var query = _db.Media.AsNoTracking();

        if (req.InBasket.HasValue && req.InBasket.Value && User.Identity != null)
            query = query.Where(m => m.Users!.Any(u => u.Name == User.Identity.Name));

        if (!User.IsInRole(C.ADMIN_ROLE))
            query = query.Where(m => !m.Hidden);

        if (req.NoCreate.HasValue)
            query = query.Where(m => m.Created.HasValue != req.NoCreate.Value);

        if (req.TagIds != null)
            query = query.Where(m => m.Tags!.Any(t => req.TagIds.Contains(t.TagId)));

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

        var userName = User.Identity?.Name?.ToLower();
        var items = await query
            .Paginate(req)
            .Select(m => new MediaLM(m.Original, m.Preview)
            {
                Id = m.MediaId,
                IsVideo = m.IsVideo,
                Created = m.Created,
                InBasket = m.Users!.Any(u => u.Name == userName),
                HasTags = m.Tags!.Any(),
                Hidden = m.Hidden,
            })
            .ToListAsync();

        return Ok(new ListResponse<MediaLM>(req, count, items));
    }

    [Authorize(Roles = C.ADMIN_ROLE)]
    [HttpGet("{mediaId:int}", Name = "GetMediaById")]
    [ProducesResponseType(typeof(MediaVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int mediaId)
    {
        var media = await _db.Media
           .AsNoTracking()
           .Where(m => m.MediaId == mediaId)
           .Include(m => m.Tags)
           .Select(m => new MediaVM(m))
           .FirstOrDefaultAsync();

        if (media == null)
            return NotFound(new PlainError("Not found"));

        return Ok(media);
    }

    [HttpGet("basket", Name = "GetBasketItems")]
    [ProducesResponseType(typeof(int[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBasketItemsAsync()
    {
        var mediaIds = await _db.Media
            .AsNoTracking()
            .Where(m => m.Users!.Any(u => u.Name == User.Identity!.Name))
            .Select(m => m.MediaId)
            .ToListAsync();

        return Ok(mediaIds);
    }

    [HttpPut("{mediaId}", Name = "UpdateMedia")]
    [ProducesResponseType(typeof(MediaVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int mediaId, MediaUM model)
    {
        var media = await _db.Media
          .Where(u => u.MediaId == mediaId)
          .FirstOrDefaultAsync();

        if (media == null)
            return NotFound(new PlainError("Not found"));

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        media.Hidden = model.Hidden;
        media.Created = model.Created;

        await _db.SaveChangesAsync();

        return Ok(new MediaVM(media));
    }

    [HttpPost("{mediaId:int}/basket", Name = "AddToBasket")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToBasketAsync(int mediaId)
    {
        var userName = User.Identity?.Name?.ToLower();
        if (string.IsNullOrWhiteSpace(userName))
            return NotFound(new PlainError("Username not found"));

        var user = await _db.Users.Include(u => u.Basket).AsSplitQuery().SingleOrDefaultAsync(u => u.Name == userName);
        if (user == null)
            return NotFound(new PlainError("Username not found"));

        var media = await _db.Media.SingleOrDefaultAsync(m => m.MediaId == mediaId);
        if (media == null)
            return NotFound(new PlainError("Media not found"));

        user.Basket!.Add(media);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = C.ADMIN_ROLE)]
    [HttpPost("{mediaId:int}/tags/{tagId:int}", Name = "AddTag")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTagAsync(int mediaId, int tagId)
    {
        var media = await _db.Media.SingleOrDefaultAsync(m => m.MediaId == mediaId);
        if (media == null)
            return NotFound(new PlainError("Media not found"));

        var tag = await _db.Tags.SingleOrDefaultAsync(u => u.TagId == tagId);
        if (tag == null)
            return NotFound(new PlainError("Tag not found"));

        tag.Media = new();
        tag.Media.Add(media);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{mediaId:int}/basket", Name = "RemoveFromBasket")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromBasketAsync(int mediaId)
    {
        var userName = User.Identity?.Name?.ToLower();
        if (string.IsNullOrWhiteSpace(userName))
            return NotFound(new PlainError("Username not found"));

        var user = await _db.Users.Include(u => u.Basket).AsSplitQuery().SingleOrDefaultAsync(u => u.Name == userName);
        if (user == null)
            return NotFound(new PlainError("Username not found"));

        var media = await _db.Media.SingleOrDefaultAsync(m => m.MediaId == mediaId);
        if (media == null)
            return NotFound(new PlainError("Media not found"));

        user.Basket!.Remove(media);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = C.ADMIN_ROLE)]
    [HttpDelete("{mediaId:int}/tags/{tagId:int}", Name = "RemoveTag")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTagAsync(int mediaId, int tagId)
    {
        var tag = await _db.Tags
            .Include(t => t.Media!.Where(m => m.MediaId == mediaId))
            .SingleOrDefaultAsync(u => u.TagId == tagId);
        if (tag == null)
            return NotFound(new PlainError("Tag not found"));
        if (tag.Media!.Count != 1)
            return NotFound(new PlainError("Media not found"));

        tag.Media!.RemoveAt(0);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = C.ADMIN_ROLE)]
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
            System.IO.File.Delete(C.Paths.PreviewDataFor(media.Preview));

        await transaction.CommitAsync();

        return NoContent();
    }

    [HttpGet("{*path}")]
    public IActionResult GetFile(string path)
    {
        var isPreview = Path.GetFileNameWithoutExtension(path).EndsWith(C.Paths.PreviewFileNameSuffix);
        var filePath = isPreview ? C.Paths.PreviewDataFor(path) : C.Paths.MediaDataFor(path);
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
public class MediaQuery : FilterQuery
{
    public bool? InBasket { get; set; }
    public bool? Hidden { get; set; }
    public bool? NoCreate { get; set; }
    public int[]? TagIds { get; set; }
}
public enum MediaSortBy
{
    Created = 0,
    IsVideo = 1,
}