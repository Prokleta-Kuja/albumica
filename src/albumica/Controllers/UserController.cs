using albumica.Entities;
using albumica.Models;
using albumica.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace albumica.Controllers;

[ApiController]
[Route("api/users")]
[Tags(nameof(User))]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class UsersController : ControllerBase
{
    readonly ILogger<UsersController> _logger;
    readonly AppDbContext _db;
    readonly HibpService _hibp;
    readonly IPasswordHasher _pwd;
    public UsersController(ILogger<UsersController> logger, AppDbContext db, HibpService hibp, IPasswordHasher pwd)
    {
        _logger = logger;
        _db = db;
        _hibp = hibp;
        _pwd = pwd;
    }

    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(ListResponse<UserLM>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] FilterQuery req)
    {
        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            query = query.Where(u => u.Name.Contains(req.SearchTerm.ToLower()) || u.DisplayName.Contains(req.SearchTerm));

        var count = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(req.SortBy) && Enum.TryParse<UsersSortBy>(req.SortBy, true, out var sortBy))
            query = sortBy switch
            {
                UsersSortBy.Name => query.Order(u => u.Name, req.Ascending),
                UsersSortBy.DisplayName => query.Order(u => u.DisplayName, req.Ascending),
                UsersSortBy.IsAdmin => query.Order(u => u.IsAdmin, req.Ascending),
                UsersSortBy.Disabled => query.Order(u => u.Disabled, req.Ascending),
                _ => query
            };

        var items = await query
            .Paginate(req)
            .Select(u => new UserLM
            {
                Id = u.UserId,
                Name = u.Name,
                DisplayName = u.DisplayName,
                IsAdmin = u.IsAdmin,
                Disabled = u.Disabled,
            })
            .ToListAsync();

        return Ok(new ListResponse<UserLM>(req, count, items));
    }

    [HttpGet("{userId}", Name = "GetUser")]
    [ProducesResponseType(typeof(UserVM), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOneAsnyc(int userId)
    {
        var user = await _db.Users
           .AsNoTracking()
           .Where(u => u.UserId == userId)
           .Select(u => new UserVM(u))
           .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new PlainError("Not found"));

        return Ok(user);
    }

    [HttpPost(Name = "CreateUser")]
    [ProducesResponseType(typeof(UserVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(UserCM model)
    {
        model.Name = model.Name.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Users
            .AsNoTracking()
            .Where(u => u.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        var hibpResult = await _hibp.CheckAsync(model.Password);
        if (!string.IsNullOrWhiteSpace(hibpResult))
            return BadRequest(new ValidationError(nameof(model.Password), hibpResult));

        var user = new User
        {
            Name = model.Name,
            DisplayName = model.DisplayName,
            IsAdmin = model.IsAdmin,
            PasswordHash = _pwd.HashPassword(model.Password),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new UserVM(user));
    }

    [HttpPut("{userId}", Name = "UpdateUser")]
    [ProducesResponseType(typeof(UserVM), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int userId, UserUM model)
    {
        var user = await _db.Users
          .Where(u => u.UserId == userId)
          .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new PlainError("Not found"));

        model.Name = model.Name.Trim().ToLower();

        if (model.IsInvalid(out var errorModel))
            return BadRequest(errorModel);

        var isDuplicate = await _db.Users
            .AsNoTracking()
            .Where(u => u.UserId != user.UserId && u.Name == model.Name)
            .AnyAsync();

        if (isDuplicate)
            return BadRequest(new ValidationError(nameof(model.Name), "Already exists"));

        user.Name = model.Name;
        user.DisplayName = model.DisplayName;
        user.IsAdmin = model.IsAdmin;
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var hibpResult = await _hibp.CheckAsync(model.NewPassword);
            if (!string.IsNullOrWhiteSpace(hibpResult))
                return BadRequest(new ValidationError(nameof(model.NewPassword), hibpResult));

            user.PasswordHash = _pwd.HashPassword(model.NewPassword);
        }
        if (model.Disabled.HasValue)
            user.Disabled = model.Disabled.Value ? user.Disabled.HasValue ? user.Disabled : DateTime.UtcNow : null;

        await _db.SaveChangesAsync();

        return Ok(new UserVM(user));
    }

    [HttpDelete("{userId}", Name = "DeleteUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int userId)
    {
        var user = await _db.Users
          .Where(u => u.UserId == userId)
          .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new PlainError("Not found"));

        using var transaction = _db.Database.BeginTransaction();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        return NoContent();
    }
}

public enum UsersSortBy
{
    Name = 0,
    DisplayName = 1,
    IsAdmin = 2,
    Disabled = 3,
}