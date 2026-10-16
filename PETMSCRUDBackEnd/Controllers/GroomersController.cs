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
public class GroomersController(PETMSDBContext db, AccountService accounts) : ControllerBase
{
    private static readonly Expression<Func<Groomer, GroomerResponse>> ToResponse = g =>
        new GroomerResponse(g.Id, g.StaffNumber, g.Name, g.Surname, g.Email, g.PhoneNumber, g.Specialization);

    // GET api/groomers?staffNumber=&name=&surname=
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroomerResponse>>> Search(
        [FromQuery] string? staffNumber, [FromQuery] string? name, [FromQuery] string? surname)
    {
        IQueryable<Groomer> q = db.Groomers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(staffNumber)) q = q.Where(g => g.StaffNumber.Contains(staffNumber));
        if (!string.IsNullOrWhiteSpace(name)) q = q.Where(g => g.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(surname)) q = q.Where(g => g.Surname.Contains(surname));
        return await q.OrderBy(g => g.Surname).ThenBy(g => g.Name).Select(ToResponse).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroomerResponse>> GetById(int id)
    {
        var g = await db.Groomers.AsNoTracking().Where(x => x.Id == id).Select(ToResponse).FirstOrDefaultAsync();
        return g is null ? NotFound() : g;
    }

    [HttpPost]
    public async Task<ActionResult<GroomerResponse>> Create(GroomerRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Password))
            return BadRequest(new { message = "Password is required when creating a groomer." });
        if (await db.Groomers.AnyAsync(g => g.StaffNumber == r.StaffNumber))
            return Conflict(new { message = $"Staff number '{r.StaffNumber}' already exists." });
        if (await db.Groomers.AnyAsync(g => g.Email == r.Email))
            return Conflict(new { message = $"Email '{r.Email}' is already registered." });

        var (user, errors) = await accounts.CreateAsync(r.Email, r.Password, $"{r.Name} {r.Surname}", Roles.Groomer);
        if (user is null) return BadRequest(new { message = string.Join(" ", errors) });

        var groomer = new Groomer
        {
            StaffNumber = r.StaffNumber, Name = r.Name, Surname = r.Surname, Email = r.Email,
            PhoneNumber = r.PhoneNumber, Specialization = r.Specialization, UserId = user.Id
        };
        db.Groomers.Add(groomer);
        try { await db.SaveChangesAsync(); }
        catch { await accounts.DeleteUserAsync(user.Id); throw; }

        var dto = await db.Groomers.Where(x => x.Id == groomer.Id).Select(ToResponse).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = groomer.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, GroomerRequest r)
    {
        var g = await db.Groomers.FindAsync(id);
        if (g is null) return NotFound();
        if (await db.Groomers.AnyAsync(x => x.StaffNumber == r.StaffNumber && x.Id != id))
            return Conflict(new { message = $"Staff number '{r.StaffNumber}' already exists." });
        if (await db.Groomers.AnyAsync(x => x.Email == r.Email && x.Id != id))
            return Conflict(new { message = $"Email '{r.Email}' is already registered." });

        g.StaffNumber = r.StaffNumber; g.Name = r.Name; g.Surname = r.Surname;
        g.Email = r.Email; g.PhoneNumber = r.PhoneNumber; g.Specialization = r.Specialization;
        await db.SaveChangesAsync();
        await accounts.UpdateAsync(g.UserId, r.Email, $"{r.Name} {r.Surname}");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var g = await db.Groomers.FindAsync(id);
        if (g is null) return NotFound();
        if (await db.Appointments.AnyAsync(a => a.GroomerId == id) || await db.GroomingRecords.AnyAsync(r => r.GroomerId == id))
            return Conflict(new { message = "Cannot delete a groomer who has appointments or grooming records." });

        db.Groomers.Remove(g);
        await db.SaveChangesAsync();
        await accounts.DeleteUserAsync(g.UserId);
        return NoContent();
    }
}
