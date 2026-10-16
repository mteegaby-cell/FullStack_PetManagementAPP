using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PETMSCRUD.Models;

public class ApplicationUser : IdentityUser 
{
    public string? FullName { get; set; }
}

public class PetOwner
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string OwnerNumber { get; set; } = "";
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    [Required, MaxLength(50)] public string Surname { get; set; } = "";
    [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = "";
    [Required, MaxLength(20)] public string PhoneNumber { get; set; } = "";
    [Required, MaxLength(200)] public string HomeAddress { get; set; } = "";

    public string? UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public class Pet
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string PetNumber { get; set; } = "";
    [Required, MaxLength(50)] public string PetName { get; set; } = "";
    [Required, MaxLength(30)] public string Species { get; set; } = "";
    [MaxLength(50)] public string? Breed { get; set; }
    public Gender Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }
    [MaxLength(30)] public string? Colour { get; set; }
    public decimal Weight { get; set; }
    [MaxLength(500)] public string? MedicalNotes { get; set; }

    public int PetOwnerId { get; set; }
    public PetOwner? Owner { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<GroomingRecord> GroomingRecords { get; set; } = new List<GroomingRecord>();
}

public class Groomer
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string StaffNumber { get; set; } = "";
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    [Required, MaxLength(50)] public string Surname { get; set; } = "";
    [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = "";
    [Required, MaxLength(20)] public string PhoneNumber { get; set; } = "";
    [Required, MaxLength(100)] public string Specialization { get; set; } = "";

    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<GroomingRecord> GroomingRecords { get; set; } = new List<GroomingRecord>();
}

public class GroomingService
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string ServiceName { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;   // "Service Status"
    public ServiceStatus Status { get; set; } = ServiceStatus.Active;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<GroomingRecord> GroomingRecords { get; set; } = new List<GroomingRecord>();
}

public class Appointment
{
    public int Id { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    [MaxLength(500)] public string? Notes { get; set; }

    public int PetId { get; set; }
    public Pet? Pet { get; set; } = null!;

    public int PetOwnerId { get; set; }
    public PetOwner? PetOwner { get; set; }

    public int? GroomerId { get; set; }          // owner requests; admin assigns the groomer
    public Groomer? Groomer { get; set; }

    public int GroomingServiceId { get; set; }
    public GroomingService GroomingService { get; set; } = null;
    public GroomingRecord? GroomingRecord { get; set; }
}

public class GroomingRecord
{
    public int Id { get; set; }
    public DateOnly GroomingDate { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    public int GroomerId { get; set; }
    public Groomer? Groomer { get; set; }
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int GroomingServiceId { get; set; }   // "Service Performed"
    public GroomingService? GroomingService { get; set; }
}
