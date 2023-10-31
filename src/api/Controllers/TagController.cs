using albumica.Entities;
using albumica.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace albumica.Controllers;

[Authorize(Roles = C.ADMIN_ROLE)]
[ApiController]
[Route("api/tags")]
[Tags(nameof(Tag))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class TagController : ControllerBase
{
    readonly ILogger<TagController> _logger;
    readonly AppDbContext _db;
    public TagController(ILogger<TagController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet(Name = "GetTags")]
    [ProducesResponseType(typeof(ListResponse<TagLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] FilterQuery req)
    {
        var query = _db.Tags.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(u => u.Name.Contains(req.SearchTerm.ToLower()));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<TagsSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                TagsSortBy.Name => query.Order(u => u.Name, req.Ascending),
                TagsSortBy.Order => query.Order(u => u.Order, req.Ascending),
                TagsSortBy.MediaCount => query.Order(u => u.Media!.Count, req.Ascending),
                _ => query
            };
        else
        {
            req.Ascending = false;
            req.SortBy = nameof(TagsSortBy.Order);
            query = query.Order(m => m.Order, req.Ascending).ThenByDescending(m => m.Media!.Count);
        }

        var items = await query
            .Paginate(req)
            .Select(t => new TagLM
            {
                Id = t.TagId,
                Name = t.Name,
                Order = t.Order,
                MediaCount = t.Media!.Count,
            })
            .ToListAsync();

        return Ok(new ListResponse<TagLM>(req, count, items));
    }

    [HttpGet("{tagId}", Name = "GetTag")]
    [ProducesResponseType(typeof(TagVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int tagId)
    {
        var tag = await _db.Tags
           .AsNoTracking()
           .Where(u => u.TagId == tagId)
           .Select(u => new TagVM(u))
           .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound(new PlainError("Not found"));

        return Ok(tag);
    }

    [HttpPost(Name = "CreateTag")]
    [ProducesResponseType(typeof(TagVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(TagCM model)
    {
        model.Name = model.Name.Trim();
        var normalized = model.Name.ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Tags
            .AsNoTracking()
            .Where(u => u.NameNormalized == normalized)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        var tag = new Tag
        {
            Name = model.Name,
            NameNormalized = normalized,
            Order = model.Order,
        };

        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();

        return Ok(new TagVM(tag));
    }

    [HttpPut("{tagId}", Name = "UpdateTag")]
    [ProducesResponseType(typeof(TagVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int tagId, TagUM model)
    {
        var tag = await _db.Tags
          .Where(u => u.TagId == tagId)
          .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound(new PlainError("Not found"));

        model.Name = model.Name.Trim();
        var normalized = model.Name.ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Tags
            .AsNoTracking()
            .Where(u => u.TagId != tag.TagId && u.NameNormalized == normalized)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        tag.Name = model.Name;
        tag.NameNormalized = normalized;
        tag.Order = model.Order;

        await _db.SaveChangesAsync();

        return Ok(new TagVM(tag));
    }

    [HttpDelete("{tagId}", Name = "DeleteTag")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int tagId)
    {
        var tag = await _db.Tags
          .Where(u => u.TagId == tagId)
          .FirstOrDefaultAsync();

        if (tag == null)
            return NotFound(new PlainError("Not found"));

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public enum TagsSortBy
{
    Name = 0,
    Order = 1,
    MediaCount = 2,
}