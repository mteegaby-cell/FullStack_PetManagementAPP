using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETMSCRUD.Data;
using PETMSCRUD.Dtos;
using PETMSCRUD.Models;

namespace PETMSCRUD.Controllers;

// Admin manages services. (Groomers/Owners only need to READ them: add a separate
// read-only endpoint with [Authorize(Roles = "Admin,Groomer,PetOwner")] if your team wants one.)
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class ServicesController(PETMSDBContext db) : ControllerBase
{
    private static readonly Expression<Func<GroomingService, ServiceResponse>> ToResponse = s =>
        new ServiceResponse(s.Id, s.ServiceName, s.Description, s.DurationMinutes, s.Price, s.Status);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> GetAll([FromQuery] ServiceStatus? status, [FromQuery] string? name)
    {
        IQueryable<GroomingService> q = db.GroomingServices.AsNoTracking();
        if (status.HasValue) q = q.Where(s => s.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(name)) q = q.Where(s => s.ServiceName.Contains(name));
        return await q.OrderBy(s => s.ServiceName).Select(ToResponse).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResponse>> GetById(int id)
    {
        var s = await db.GroomingServices.AsNoTracking().Where(x => x.Id == id).Select(ToResponse).FirstOrDefaultAsync();
        return s is null ? NotFound() : s;
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(ServiceRequest r)
    {
        if (await db.GroomingServices.AnyAsync(s => s.ServiceName == r.ServiceName))
            return Conflict(new { message = $"A service named '{r.ServiceName}' already exists." });

        var s = new GroomingService
        {
            ServiceName = r.ServiceName, Description = r.Description,
            DurationMinutes = r.DurationMinutes, Price = r.Price, Status = r.Status
        };
        db.GroomingServices.Add(s);
        await db.SaveChangesAsync();
        var dto = await db.GroomingServices.Where(x => x.Id == s.Id).Select(ToResponse).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = s.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ServiceRequest r)
    {
        var s = await db.GroomingServices.FindAsync(id);
        if (s is null) return NotFound();
        if (await db.GroomingServices.AnyAsync(x => x.ServiceName == r.ServiceName && x.Id != id))
            return Conflict(new { message = $"A service named '{r.ServiceName}' already exists." });

        s.ServiceName = r.ServiceName; s.Description = r.Description;
        s.DurationMinutes = r.DurationMinutes; s.Price = r.Price; s.Status = r.Status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await db.GroomingServices.FindAsync(id);
        if (s is null) return NotFound();
        if (await db.Appointments.AnyAsync(a => a.GroomingServiceId == id) || await db.GroomingRecords.AnyAsync(g => g.GroomingServiceId == id))
            return Conflict(new { message = "This service is used by appointments/records. Set its status to Inactive instead." });

        db.GroomingServices.Remove(s);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
