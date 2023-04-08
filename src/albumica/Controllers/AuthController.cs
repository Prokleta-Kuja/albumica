using System.Security.Claims;
using albumica.Entities;
using albumica.Models;
using albumica.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace albumica.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(PlainError))]
public class AuthController : ControllerBase
{
    readonly ILogger<AuthController> _logger;
    readonly AppDbContext _db;
    readonly IPasswordHasher _pwd;

    public AuthController(ILogger<AuthController> logger, AppDbContext db, IPasswordHasher pwd)
    {
        _logger = logger;
        _db = db;
        _pwd = pwd;
    }

    [HttpGet(Name = "Status")]
    [ProducesResponseType(typeof(AuthStatusModel), StatusCodes.Status200OK)]
    public IActionResult Status()
    {
        if (HttpContext.User.Identity?.IsAuthenticated ?? false)
        {
            var expires = DateTime.MinValue;
            var expiresStr = HttpContext.User.FindFirst(ClaimTypes.Expiration)?.Value;
            if (!string.IsNullOrWhiteSpace(expiresStr) && long.TryParse(expiresStr, out var expiresVal))
                expires = DateTime.FromBinary(expiresVal);

            return Ok(new AuthStatusModel { Authenticated = true, Username = HttpContext.User.Identity!.Name, Expires = expires });
        }
        else
            return Ok(new AuthStatusModel { Authenticated = false });
    }

    [HttpPatch(Name = "AutoLogin")]
    [ProducesResponseType(typeof(AuthStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AutoLoginAsync()
    {
        var hasMasters = await _db.Users.AsNoTracking().AnyAsync(u => u.IsAdmin);
        if (hasMasters)
            return BadRequest(new PlainError("There are master users in database, autologin disabled."));

        _logger.LogInformation("No master users in database, autologing in...");
        var expires = DateTime.UtcNow.AddMinutes(10);
        var claims = new List<Claim> { new(ClaimTypes.Name, "temporary admin"), new(ClaimTypes.Role, C.ADMIN_ROLE), new(ClaimTypes.Expiration, expires.ToBinary().ToString()) };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { AllowRefresh = false, ExpiresUtc = expires, IsPersistent = true };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok(new AuthStatusModel { Authenticated = true, Username = "temporary admin", Expires = expires });
    }

    [HttpPost(Name = "Login")]
    [ProducesResponseType(typeof(AuthStatusModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginAsync(LoginModel model)
    {
        model.Username = model.Username.ToLower();
        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Name == model.Username && u.IsAdmin == true && u.Disabled == null);
        if (user == null || _pwd.VerifyHashedPassword(user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
            return BadRequest(new PlainError("Invalid username or password"));

        var expires = DateTime.UtcNow.AddHours(8);
        var claims = new List<Claim> { new(ClaimTypes.Name, user.Name), new(ClaimTypes.Sid, user.UserId.ToString()), new(ClaimTypes.Expiration, expires.ToBinary().ToString()) };
        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, C.ADMIN_ROLE));

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { AllowRefresh = false, ExpiresUtc = expires, IsPersistent = true };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        return Ok(new AuthStatusModel { Authenticated = true, Username = user.Name, Expires = expires });
    }

    [HttpDelete(Name = "Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LogoutAsync()
    {
        if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}