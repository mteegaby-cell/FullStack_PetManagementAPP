using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETMSCRUD.Data;
using PETMSCRUD.Dtos;
using PETMSCRUD.Models;

namespace PETMSCRUD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Groomer)]
public class AppointmentsController(PETMSDBContext db) : ControllerBase
{
    // Salon opening hours (minutes from midnight). Adjust to suit Paws & Care.
    private const int OpenMinute = 8 * 60;
    private const int CloseMinute = 17 * 60;

    private static readonly Expression<Func<Appointment, AppointmentResponse>> ToResponse = a =>
        new AppointmentResponse(a.Id, a.AppointmentDate, a.AppointmentTime,
            a.PetId, a.Pet.PetName, a.PetOwnerId, a.PetOwner.Name + " " + a.PetOwner.Surname,
            a.GroomerId.Value, a.Groomer.Name + " " + a.Groomer.Surname,
            a.GroomingServiceId, a.GroomingService.ServiceName, a.GroomingService.DurationMinutes, a.GroomingService.Price,
            a.Status, a.Notes);

    // GET api/appointments?status=Scheduled&date=2026-10-20&groomerId=1&petId=2   (status filtering)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAll(
        [FromQuery] AppointmentStatus? status, [FromQuery] DateOnly? date,
        [FromQuery] int? groomerId, [FromQuery] int? petId)
    {
        IQueryable<Appointment> q = db.Appointments.AsNoTracking();
        if (status.HasValue) q = q.Where(a => a.Status == status.Value);
        if (date.HasValue) q = q.Where(a => a.AppointmentDate == date.Value);
        if (groomerId.HasValue) q = q.Where(a => a.GroomerId == groomerId.Value);
        if (petId.HasValue) q = q.Where(a => a.PetId == petId.Value);

        return await q.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime).Select(ToResponse).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentResponse>> GetById(int id)
    {
        var a = await db.Appointments.AsNoTracking().Where(x => x.Id == id).Select(ToResponse).FirstOrDefaultAsync();
        return a is null ? NotFound() : a;
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Create(AppointmentRequest r)
    {
        var (error, pet) = await ValidateAsync(r, null, isNew: true);
        if (error is not null) return BadRequest(new { message = error });

        var a = new Appointment
        {
            AppointmentDate = r.AppointmentDate, AppointmentTime = r.AppointmentTime,
            PetId = r.PetId, PetOwnerId = pet!.PetOwnerId,      // owner is derived from the pet => always consistent
            GroomerId = r.GroomerId, GroomingServiceId = r.ServiceId, Status = r.Status, Notes = r.Notes
        };
        db.Appointments.Add(a);
        await db.SaveChangesAsync();
        var dto = await db.Appointments.Where(x => x.Id == a.Id).Select(ToResponse).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = a.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppointmentRequest r)
    {
        var a = await db.Appointments.FindAsync(id);
        if (a is null) return NotFound();

        var (error, pet) = await ValidateAsync(r, id, isNew: false);
        if (error is not null) return BadRequest(new { message = error });

        a.AppointmentDate = r.AppointmentDate; a.AppointmentTime = r.AppointmentTime;
        a.PetId = r.PetId; a.PetOwnerId = pet!.PetOwnerId;
        a.GroomerId = r.GroomerId; a.Id = r.ServiceId; a.Status = r.Status; a.Notes = r.Notes;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await db.Appointments.FindAsync(id);
        if (a is null) return NotFound();
        if (await db.GroomingRecords.AnyAsync(g => g.Id == id))
            return Conflict(new { message = "This appointment has a grooming record. Cancel it instead of deleting." });

        db.Appointments.Remove(a);
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Prevents inappropriate / conflicting allocations.</summary>
    private async Task<(string? Error, Pet? Pet)> ValidateAsync(AppointmentRequest r, int? excludeId, bool isNew)
    {
        var pet = await db.Pets.FindAsync(r.PetId);
        if (pet is null) return ("Selected pet does not exist.", null);

        var groomer = await db.Groomers.FindAsync(r.GroomerId);
        if (groomer is null) return ("Selected groomer does not exist.", null);

        var service = await db.GroomingServices.FindAsync(r.ServiceId);
        if (service is null) return ("Selected service does not exist.", null);
        if (service.Status != ServiceStatus.Active && r.Status != AppointmentStatus.Cancelled)
            return ("Selected service is inactive and cannot be booked.", null);

        // Cancelled appointments don't occupy a slot, so skip the time checks.
        if (r.Status == AppointmentStatus.Cancelled) return (null, pet);

        if (isNew && r.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
            return ("Appointments cannot be booked in the past.", null);

        int start = r.AppointmentTime.Hour * 60 + r.AppointmentTime.Minute;
        int end = start + service.DurationMinutes;
        if (start < OpenMinute || end > CloseMinute)
            return ($"Appointment must fall within opening hours ({OpenMinute / 60:00}:00-{CloseMinute / 60:00}:00).", null);

        // Same-day, non-cancelled appointments for this groomer OR this pet.
        int exclude = excludeId ?? 0;
        var sameDay = await db.Appointments.Include(x => x.GroomingService)
            .Where(x => x.AppointmentDate == r.AppointmentDate
                     && x.Status != AppointmentStatus.Cancelled
                     && x.Id != exclude
                     && (x.GroomerId == r.GroomerId || x.PetId == r.PetId))
            .ToListAsync();

        foreach (var x in sameDay)
        {
            int xs = x.AppointmentTime.Hour * 60 + x.AppointmentTime.Minute;
            int xe = xs + x.GroomingService.DurationMinutes;
            bool overlaps = start < xe && xs < end;             // classic interval-overlap test
            if (!overlaps) continue;

            var window = $"{TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(xs)):HH:mm}-{TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(xe)):HH:mm}";
            if (x.GroomerId == r.GroomerId) return ($"Groomer is already booked from {window} on that day.", null);
            return ($"This pet already has an appointment from {window} on that day.", null);
        }
        return (null, pet);
    }
}
