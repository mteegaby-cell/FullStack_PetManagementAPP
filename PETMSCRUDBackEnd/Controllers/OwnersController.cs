using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETMSCRUD.Data;
using PETMSCRUD.Dtos;
using PETMSCRUD.Models;
using PETMSCRUD.Services;

namespace PETMSCRUD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class OwnersController(PETMSDBContext db, AccountService accounts) : ControllerBase
{
    private static readonly Expression<Func<PetOwner, OwnerResponse>> ToResponse = o =>
        new OwnerResponse(o.Id, o.OwnerNumber, o.Name, o.Surname, o.Email, o.PhoneNumber, o.HomeAddress, o.Pets.Count);

    // GET api/owners?ownerNumber=&name=&surname=   (all filters optional, combinable)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OwnerResponse>>> Search(
        [FromQuery] string? ownerNumber, [FromQuery] string? name, [FromQuery] string? surname)
    {
        IQueryable<PetOwner> q = db.PetOwners.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(ownerNumber)) q = q.Where(o => o.OwnerNumber.Contains(ownerNumber));
        if (!string.IsNullOrWhiteSpace(name)) q = q.Where(o => o.Name.Contains(name));       // -> LIKE '%name%'
        if (!string.IsNullOrWhiteSpace(surname)) q = q.Where(o => o.Surname.Contains(surname));

        return await q.OrderBy(o => o.Surname).ThenBy(o => o.Name).Select(ToResponse).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OwnerResponse>> GetById(int id)
    {
        var o = await db.PetOwners.AsNoTracking().Where(x => x.Id == id).Select(ToResponse).FirstOrDefaultAsync();
        return o is null ? NotFound() : o;
    }

    [HttpPost]
    public async Task<ActionResult<OwnerResponse>> Create(OwnerRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Password))
            return BadRequest(new { message = "Password is required when creating an owner." });
        if (await db.PetOwners.AnyAsync(o => o.OwnerNumber == r.OwnerNumber))
            return Conflict(new { message = $"Owner number '{r.OwnerNumber}' already exists." });
        if (await db.PetOwners.AnyAsync(o => o.Email == r.Email))
            return Conflict(new { message = $"Email '{r.Email}' is already registered." });

        var (user, errors) = await accounts.CreateAsync(r.Email, r.Password, $"{r.Name} {r.Surname}", Roles.PetOwner);
        if (user is null) return BadRequest(new { message = string.Join(" ", errors) });

        var owner = new PetOwner
        {
            OwnerNumber = r.OwnerNumber, Name = r.Name, Surname = r.Surname, Email = r.Email,
            PhoneNumber = r.PhoneNumber, HomeAddress = r.HomeAddress, UserId = user.Id
        };
        db.PetOwners.Add(owner);
        try { await db.SaveChangesAsync(); }
        catch { await accounts.DeleteUserAsync(user.Id); throw; }   // don't leave an orphan login

        var dto = await db.PetOwners.Where(x => x.Id == owner.Id).Select(ToResponse).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = owner.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, OwnerRequest r)
    {
        var owner = await db.PetOwners.FindAsync(id);
        if (owner is null) return NotFound();
        if (await db.PetOwners.AnyAsync(o => o.OwnerNumber == r.OwnerNumber && o.Id != id))
            return Conflict(new { message = $"Owner number '{r.OwnerNumber}' already exists." });
        if (await db.PetOwners.AnyAsync(o => o.Email == r.Email && o.Id != id))
            return Conflict(new { message = $"Email '{r.Email}' is already registered." });

        owner.OwnerNumber = r.OwnerNumber; owner.Name = r.Name; owner.Surname = r.Surname;
        owner.Email = r.Email; owner.PhoneNumber = r.PhoneNumber; owner.HomeAddress = r.HomeAddress;
        await db.SaveChangesAsync();
        await accounts.UpdateAsync(owner.UserId, r.Email, $"{r.Name} {r.Surname}");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var owner = await db.PetOwners.FindAsync(id);
        if (owner is null) return NotFound();
        if (await db.Pets.AnyAsync(p => p.PetOwnerId == id) || await db.Appointments.AnyAsync(a => a.PetOwnerId == id))
            return Conflict(new { message = "Cannot delete an owner who still has pets or appointments. Remove those first." });

        db.PetOwners.Remove(owner);
        await db.SaveChangesAsync();
        await accounts.DeleteUserAsync(owner.UserId);
        return NoContent();
    }
}
