using System.ComponentModel.DataAnnotations;
using PETMSCRUD.Data;
using PETMSCRUD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSCRUD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Groomer")]   // PERSON 4: uncomment after Identity is merged
    public class GroomerController : ControllerBase
    {
        private readonly PETMSDBContext dbContext;

        public GroomerController(PETMSDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // ------------------------------------------------------------------
        // TEMPORARY: the groomer is picked by the X-Dev-GroomerId header
        // (default = groomer 1).
        // PERSON 4: replace the body with:
        //   var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //   return await dbContext.Groomers.FirstOrDefaultAsync(g => g.UserId == userId);
        // (add: using System.Security.Claims;)
        // ------------------------------------------------------------------
        private async Task<Groomer?> GetCurrentGroomer()
        {
            int id = int.TryParse(Request.Headers["X-Dev-GroomerId"], out var h) ? h : 1;
            return await dbContext.Groomers.FirstOrDefaultAsync(g => g.Id == id);
        }

        // A groomer may only see a pet they have an appointment for
        private Task<bool> HasAccessToPet(int groomerId, int petId) =>
            dbContext.Appointments.AnyAsync(a => a.GroomerId == groomerId && a.PetId == petId);

        // GET api/groomer/me
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var g = await GetCurrentGroomer();
            if (g == null) return Unauthorized();
            return Ok(new { g.Id, g.StaffNumber, g.Name, g.Surname, g.Email, g.PhoneNumber, g.Specialization });
        }

        // GET api/groomer/appointments?status=Scheduled
        [HttpGet("appointments")]
        public async Task<IActionResult> MyAppointments([FromQuery] AppointmentStatus? status)
        {
            var groomer = await GetCurrentGroomer();
            if (groomer == null) return Unauthorized();

            var q = dbContext.Appointments.Where(a => a.GroomerId == groomer.Id);
            if (status.HasValue) q = q.Where(a => a.Status == status.Value);

            var list = await q.OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    Status = a.Status.ToString(),
                    a.Notes,
                    a.PetId,
                    PetName = a.Pet.PetName,
                    OwnerName = a.PetOwner.Name + " " + a.PetOwner.Surname,
                    ServiceName = a.GroomingService.ServiceName,
                    a.GroomingService.DurationMinutes
                }).ToListAsync();
            return Ok(list);
        }

        // GET api/groomer/appointments/5
        [HttpGet("appointments/{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var groomer = await GetCurrentGroomer();
            if (groomer == null) return Unauthorized();

            var a = await dbContext.Appointments
                .Where(x => x.Id == id && x.GroomerId == groomer.Id)
                .Select(x => new
                {
                    x.Id,
                    x.AppointmentDate,
                    x.AppointmentTime,
                    Status = x.Status.ToString(),
                    x.Notes,
                    x.PetId,
                    PetName = x.Pet.PetName,
                    ServiceName = x.GroomingService.ServiceName,
                    x.GroomingService.Description,
                    x.GroomingService.DurationMinutes
                }).FirstOrDefaultAsync();
            return a == null ? NotFound() : Ok(a);
        }

        public record StatusDto([Required] string Status);

        // PUT api/groomer/appointments/5/status
        [HttpPut("appointments/{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusDto dto)
        {
            var groomer = await GetCurrentGroomer();
            if (groomer == null) return Unauthorized();

            if (!Enum.TryParse<AppointmentStatus>(dto.Status, true, out var s))
                return BadRequest("Invalid status.");

            var appt = await dbContext.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.GroomerId == groomer.Id);
            if (appt == null) return NotFound();   // not theirs = not found

            appt.Status = s;
            await dbContext.SaveChangesAsync();
            return Ok(new { appt.Id, Status = appt.Status.ToString() });
        }

        // GET api/groomer/pets/1
        [HttpGet("pets/{petId}")]
        public async Task<IActionResult> GetPet(int petId)
        {
            var groomer = await GetCurrentGroomer();
            if (groomer == null) return Unauthorized();
            if (!await HasAccessToPet(groomer.Id, petId)) return NotFound();

            var pet = await dbContext.Pets.Where(p => p.Id == petId)
                .Select(p => new
                {
                    p.Id,
                    p.PetName,
                    p.Species,
                    p.Breed,
                    p.Gender,
                    p.DateOfBirth,
                    p.Colour,
                    p.Weight,
                    p.MedicalNotes,
                    OwnerName = p.Owner.Name + " " + p.Owner.Surname
                }).FirstOrDefaultAsync();
            return pet == null ? NotFound() : Ok(pet);
        }

        // GET api/groomer/pets/1/history
        [HttpGet("pets/{petId}/history")]
        public async Task<IActionResult> PetHistory(int petId)
        {
            var groomer = await GetCurrentGroomer();
            if (groomer == null) return Unauthorized();
            if (!await HasAccessToPet(groomer.Id, petId)) return NotFound();

            var history = await dbContext.GroomingRecords.Where(r => r.PetId == petId)
                .OrderByDescending(r => r.GroomingDate)
                .Select(r => new
                {
                    r.Id,
                    r.GroomingDate,
                    r. GroomingService,
                    r.Notes,
                    GroomerName = r.Groomer.Name + " " + r.Groomer.Surname
                }).ToListAsync();
            return Ok(history);
        }

        // GET api/groomer/pets/1/appointments  (this groomer's appointments for the pet)
        [HttpGet("pets/{petId}/appointments")]
        public async Task<IActionResult> PetAppointments(int petId)
        {
            var groomer = await GetCurrentGroomer();
            if (groomer == null) return Unauthorized();

            var list = await dbContext.Appointments
                .Where(a => a.PetId == petId && a.GroomerId == groomer.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    Status = a.Status.ToString(),
                    ServiceName = a.Groo.ServiceName
                }).ToListAsync();
            return Ok(list);
        }

        public class CreateRecordDto
        {
            [Required] public int PetId { get; set; }
            public int? AppointmentId { get; set; }
            [Required] public DateTime GroomingDate { get; set; }
            [Required, MaxLength(200)] public string ServicePerformed { get; set; } = "";
            [MaxLength(1000)] public string? Notes { get; set; }
        }

        // POST api/groomer/records
        [HttpPost("records")]
        public async Task<IActionResult> CreateRecord([FromBody] CreateRecordDto dto)
        {
            var groomer = await GetCurrentGroomer();
            if (groomer == null) return Unauthorized();
            if (!await HasAccessToPet(groomer.Id, dto.PetId)) return NotFound();

            if (dto.AppointmentId.HasValue)
            {
                bool ok = await dbContext.Appointments.AnyAsync(a =>
                    a.Id == dto.AppointmentId && a.GroomerId == groomer.Id && a.PetId == dto.PetId);
                if (!ok) return BadRequest("Appointment does not belong to this groomer and pet.");
            }

            var record = new GroomingRecord
            {
                GroomingDate = dto.GroomingDate,
                GroomingService = dto.ServicePerformed.Trim(),
                Notes = dto.Notes?.Trim(),
                PetId = dto.PetId,
                AppointmentId = dto.AppointmentId,
                GroomerId = groomer.Id               // set server-side, never from the client
            };
            dbContext.GroomingRecords.Add(record);
            await dbContext.SaveChangesAsync();
            return Ok(new { record.Id });
        }
    }
}