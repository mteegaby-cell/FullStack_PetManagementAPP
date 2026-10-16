using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PETMSCRUD.Data;
using PETMSCRUD.Dtos;
using PETMSCRUD.Models;

namespace PETMSCRUD.Controllers;

[ApiController]
[Route("api/owner")]
[Authorize(Roles = "PetOwner")]          // only tokens carrying the PetOwner role get in
public class OwnerController : ControllerBase
{
    private readonly PETMSDBContext _db;
    public OwnerController(PETMSDBContext db) => _db = db;

    // The owner id is NEVER taken from the request. It is resolved from the logged-in user's
    // Identity id (JWT claim), so an owner can only ever reach their own data.
    private async Task<int?> CurrentOwnerIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return null;
        return await _db.PetOwners.Where(o => o.UserId == userId)
            .Select(o => (int?)o.Id).FirstOrDefaultAsync();
    }

    // GET api/owner/me
    [HttpGet("me")]
    public async Task<ActionResult<OwnerProfileDto>> GetProfile()
    {
        var ownerId = await CurrentOwnerIdAsync();
        if (ownerId is null) return Forbid();

        return await _db.PetOwners.AsNoTracking().Where(o => o.Id == ownerId)
            .Select(o => new OwnerProfileDto(o.OwnerNumber, o.Name, o.Surname,
                o.Email, o.PhoneNumber, o.HomeAddress))
            .FirstAsync();
    }

    // GET api/owner/pets
    [HttpGet("pets")]
    public async Task<ActionResult<List<OwnerPetDto>>> GetPets()
    {
        var ownerId = await CurrentOwnerIdAsync();
        if (ownerId is null) return Forbid();

        return await _db.Pets.AsNoTracking().Where(p => p.PetOwnerId == ownerId)
            .OrderBy(p => p.PetName)
            .Select(p => new OwnerPetDto(p.Id, p.PetNumber, p.PetName, p.Species, p.Breed,
                p.Gender.ToString(), p.DateOfBirth, p.Colour, p.Weight, p.MedicalNotes))
            .ToListAsync();
    }

    // GET api/owner/pets/5   (404 if the pet belongs to someone else)
    [HttpGet("pets/{id:int}")]
    public async Task<ActionResult<OwnerPetDto>> GetPet(int id)
    {
        var ownerId = await CurrentOwnerIdAsync();
        if (ownerId is null) return Forbid();

        var pet = await _db.Pets.AsNoTracking()
            .Where(p => p.Id == id && p.PetOwnerId == ownerId)
            .Select(p => new OwnerPetDto(p.Id, p.PetNumber, p.PetName, p.Species, p.Breed,
                p.Gender.ToString(), p.DateOfBirth, p.Colour, p.Weight, p.MedicalNotes))
            .FirstOrDefaultAsync();

        return pet is null ? NotFound() : pet;
    }

    // GET api/owner/services   (active services only)
    [HttpGet("services")]
    public async Task<ActionResult<List<ServiceDto>>> GetServices()
    {
        return await _db.GroomingServices.AsNoTracking().Where(s => s.IsActive)
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceDto(s.Id, s.ServiceName, s.Description, s.DurationMinutes, s.Price))
            .ToListAsync();
    }

    // GET api/owner/appointments/upcoming?status=Scheduled
    [HttpGet("appointments/upcoming")]
    public async Task<ActionResult<List<OwnerAppointmentDto>>> GetUpcoming([FromQuery] AppointmentStatus? status)
    {
        var ownerId = await CurrentOwnerIdAsync();
        if (ownerId is null) return Forbid();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var q = _db.Appointments.AsNoTracking()
            .Where(a => a.PetOwnerId == ownerId && a.AppointmentDate >= today
                     && a.Status != AppointmentStatus.Completed
                     && a.Status != AppointmentStatus.Cancelled);
        if (status is not null) q = q.Where(a => a.Status == status);

        return await Project(q.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime))
            .ToListAsync();
    }

    // GET api/owner/appointments/history?status=Completed
    [HttpGet("appointments/history")]
    public async Task<ActionResult<List<OwnerAppointmentDto>>> GetHistory([FromQuery] AppointmentStatus? status)
    {
        var ownerId = await CurrentOwnerIdAsync();
        if (ownerId is null) return Forbid();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var q = _db.Appointments.AsNoTracking()
            .Where(a => a.PetOwnerId == ownerId &&
                    (a.AppointmentDate < today
                     || a.Status == AppointmentStatus.Completed
                     || a.Status == AppointmentStatus.Cancelled));
        if (status is not null) q = q.Where(a => a.Status == status);

        return await Project(q.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime))
            .ToListAsync();
    }

    // GET api/owner/pets/5/grooming-history
    [HttpGet("pets/{petId:int}/grooming-history")]
    public async Task<ActionResult<List<GroomingHistoryDto>>> GetGroomingHistory(int petId)
    {
        var ownerId = await CurrentOwnerIdAsync();
        if (ownerId is null) return Forbid();

        var owns = await _db.Pets.AnyAsync(p => p.Id == petId && p.PetOwnerId == ownerId);
        if (!owns) return NotFound();

        return await _db.GroomingRecords.AsNoTracking().Where(r => r.PetId == petId)
            .OrderByDescending(r => r.GroomingDate)
            .Select(r => new GroomingHistoryDto(r.Id, r.GroomingDate,
                r.GroomingService!.ServiceName, r.Notes,
                r.Groomer!.Name + " " + r.Groomer.Surname))
            .ToListAsync();
    }

    // POST api/owner/appointments   (request/book; admin assigns the groomer later)
    [HttpPost("appointments")]
    public async Task<ActionResult<OwnerAppointmentDto>> Book(BookAppointmentDto dto)
    {
        var ownerId = await CurrentOwnerIdAsync();
        if (ownerId is null) return Forbid();

        // 1. The pet must belong to this owner
        var ownsPet = await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.PetOwnerId == ownerId);
        if (!ownsPet) return BadRequest(new { message = "Selected pet was not found on your account." });

        // 2. The service must exist and be active
        var service = await _db.GroomingServices
            .FirstOrDefaultAsync(s => s.Id == dto.GroomingServiceId && s.IsActive);
        if (service is null) return BadRequest(new { message = "Selected service is not available." });

        // 3. Must be in the future
        var start = dto.AppointmentDate.ToDateTime(dto.AppointmentTime);
        if (start <= DateTime.Now)
            return BadRequest(new { message = "Appointment must be in the future." });

        // 4. Conflict check: the pet may not have an overlapping, non-cancelled appointment
        var end = start.AddMinutes(service.DurationMinutes);
        var sameDay = await _db.Appointments.AsNoTracking()
            .Where(a => a.PetId == dto.PetId && a.AppointmentDate == dto.AppointmentDate
                     && a.Status != AppointmentStatus.Cancelled)
            .Select(a => new { a.AppointmentTime, a.GroomingService!.DurationMinutes })
            .ToListAsync();

        var clash = sameDay.Any(a =>
        {
            var s = dto.AppointmentDate.ToDateTime(a.AppointmentTime);
            return start < s.AddMinutes(a.DurationMinutes) && s < end;
        });
        if (clash)
            return Conflict(new { message = "This pet already has an appointment at that time." });

        var appt = new Appointment
        {
            AppointmentDate = dto.AppointmentDate,
            AppointmentTime = dto.AppointmentTime,
            PetId = dto.PetId,
            PetOwnerId = ownerId.Value,
            GroomingServiceId = dto.GroomingServiceId,
            Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled
        };
        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        var created = await Project(_db.Appointments.AsNoTracking().Where(a => a.Id == appt.Id)).FirstAsync();
        return CreatedAtAction(nameof(GetUpcoming), created);
    }

    private static IQueryable<OwnerAppointmentDto> Project(IQueryable<Appointment> q) =>
        q.Select(a => new OwnerAppointmentDto(
            a.Id, a.AppointmentDate, a.AppointmentTime, a.Status.ToString(), a.Notes,
            a.PetId, a.Pet!.PetName, a.GroomingService!.ServiceName,
            a.GroomingService.DurationMinutes, a.GroomingService.Price,
            a.Groomer == null ? null : a.Groomer.Name + " " + a.Groomer.Surname));
}
