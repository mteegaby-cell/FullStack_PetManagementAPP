using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETMSCRUD.Data;
using PETMSCRUD.Dtos;
using PETMSCRUD.Models;
using PETMSCRUD.Services;

namespace PETMSCRUD.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly PETMSDBContext _db;
    private readonly TokenService _tokens;

    public AuthController(UserManager<ApplicationUser> users, PETMSDBContext db, TokenService tokens)
    {
        _users = users; _db = db; _tokens = tokens;
    }

    // POST api/auth/login  -> returns a JWT containing the user's roles
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var roles = (await _users.GetRolesAsync(user)).ToList();
        var (token, expires) = _tokens.Create(user, roles);

        var displayName =
            await _db.PetOwners.Where(o => o.UserId == user.Id).Select(o => o.Name + " " + o.Surname).FirstOrDefaultAsync()
            ?? await _db.Groomers.Where(g => g.UserId == user.Id).Select(g => g.Name + " " + g.Surname).FirstOrDefaultAsync()
            ?? user.Email!;

        return new AuthResponseDto(token, expires, displayName, user.Email!, roles);
    }

    // POST api/auth/register -> self-registration always creates a PetOwner
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _users.FindByEmailAsync(dto.Email) is not null)
            return Conflict(new { message = "An account with this email already exists." });

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, PhoneNumber = dto.PhoneNumber };
        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(" ", result.Errors.Select(e => e.Description)) });

        await _users.AddToRoleAsync(user, Roles.PetOwner);

        var owner = new PetOwner
        {
            OwnerNumber = Guid.NewGuid().ToString("N")[..12],   // temporary, replaced below
            Name = dto.Name, Surname = dto.Surname, Email = dto.Email,
            PhoneNumber = dto.PhoneNumber, HomeAddress = dto.HomeAddress,
            UserId = user.Id
        };
        _db.PetOwners.Add(owner);
        await _db.SaveChangesAsync();

        owner.OwnerNumber = $"OWN{owner.Id:D4}";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Registration successful. You can now log in." });
    }
}
