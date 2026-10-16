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
[Authorize(Roles = Roles.Admin)]
public class PetsController(PETMSDBContext db) : ControllerBase
{
    private static readonly Expression<Func<Pet, PetResponse>> ToResponse = p =>
        new PetResponse(p.Id, p.PetNumber, p.PetName, p.Species, p.Breed, p.Gender, p.DateOfBirth,
            p.Colour, p.Weight, p.MedicalNotes, p.PetOwnerId, p.Owner.Name + " " + p.Owner.Surname);

    // GET api/pets?petNumber=&petName=&ownerName=&ownerId=
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetResponse>>> Search(
        [FromQuery] string? petNumber, [FromQuery] string? petName,
        [FromQuery] string? ownerName, [FromQuery] int? ownerId)
    {
        IQueryable<Pet> q = db.Pets.AsNoTracking();   // Owner is reached through the navigation property -> JOIN

        if (!string.IsNullOrWhiteSpace(petNumber)) q = q.Where(p => p.PetNumber.Contains(petNumber));
        if (!string.IsNullOrWhiteSpace(petName)) q = q.Where(p => p.PetName.Contains(petName));
        if (!string.IsNullOrWhiteSpace(ownerName))
            q = q.Where(p => p.Owner.Name.Contains(ownerName) || p.Owner.Surname.Contains(ownerName)
                          || (p.Owner.Name + " " + p.Owner.Surname).Contains(ownerName));
        if (ownerId.HasValue) q = q.Where(p => p.PetOwnerId == ownerId.Value);

        return await q.OrderBy(p => p.PetName).Select(ToResponse).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PetResponse>> GetById(int id)
    {
        var p = await db.Pets.AsNoTracking().Where(x => x.Id == id).Select(ToResponse).FirstOrDefaultAsync();
        return p is null ? NotFound() : p;
    }

    [HttpPost]
    public async Task<ActionResult<PetResponse>> Create(PetRequest r)
    {
        var error = await ValidateAsync(r, null);
        if (error is not null) return error;

        var pet = new Pet();
        Apply(pet, r);
        db.Pets.Add(pet);
        await db.SaveChangesAsync();
        var dto = await db.Pets.Where(x => x.Id == pet.Id).Select(ToResponse).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PetRequest r)
    {
        var pet = await db.Pets.FindAsync(id);
        if (pet is null) return NotFound();
        var error = await ValidateAsync(r, id);
        if (error is not null) return error;

        Apply(pet, r);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await db.Pets.FindAsync(id);
        if (pet is null) return NotFound();
        if (await db.Appointments.AnyAsync(a => a.PetId == id) || await db.GroomingRecords.AnyAsync(g => g.PetId == id))
            return Conflict(new { message = "Cannot delete a pet that has appointments or grooming records." });

        db.Pets.Remove(pet);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<ActionResult?> ValidateAsync(PetRequest r, int? excludeId)
    {
        int exclude = excludeId ?? 0;
        if (r.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { message = "Date of birth cannot be in the future." });
        if (!await db.PetOwners.AnyAsync(o => o.Id == r.OwnerId))
            return BadRequest(new { message = "Selected owner does not exist." });
        if (await db.Pets.AnyAsync(p => p.PetNumber == r.PetNumber && p.Id != exclude))
            return Conflict(new { message = $"Pet number '{r.PetNumber}' already exists." });
        return null;
    }

    private static void Apply(Pet p, PetRequest r)
    {
        p.PetNumber = r.PetNumber; p.PetName = r.PetName; p.Species = r.Species; p.Breed = r.Breed;
        p.Gender = r.Gender; p.DateOfBirth = r.DateOfBirth; p.Colour = r.Colour; p.Weight = r.Weight;
        p.MedicalNotes = r.MedicalNotes; p.PetOwnerId = r.OwnerId;
    }
}
