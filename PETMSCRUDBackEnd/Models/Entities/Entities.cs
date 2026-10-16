using Microsoft.AspNetCore.Identity;

namespace PETMSCRUD.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}

public class PetOwner
{
    public int Id { get; set; }
    public string OwnerNumber { get; set; } = "";   // unique business key
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string HomeAddress { get; set; } = "";

    // Link to the Identity login account
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();               // 1 owner -> many pets
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public class Groomer
{
    public int Id { get; set; }
    public string StaffNumber { get; set; } = "";   // unique business key
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Specialization { get; set; } = "";

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();       // 1 groomer -> many appointments
    public ICollection<GroomingRecord> GroomingRecords { get; set; } = new List<GroomingRecord>();
}

public class Pet
{
    public int Id { get; set; }
    public string PetNumber { get; set; } = "";     // unique business key
    public string PetName { get; set; } = "";
    public string Species { get; set; } = "";
    public string Breed { get; set; } = "";
    public Gender Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Colour { get; set; } = "";
    public decimal Weight { get; set; }             // kg
    public string? MedicalNotes { get; set; }

    public int OwnerId { get; set; }
    public PetOwner Owner { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();       // 1 pet -> many appointments
    public ICollection<GroomingRecord> GroomingRecords { get; set; } = new List<GroomingRecord>(); // 1 pet -> many records
}

public class GroomingService
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = "";
    public string Description { get; set; } = "";
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public ServiceStatus Status { get; set; } = ServiceStatus.Active;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();       // 1 service -> many appointments
    public ICollection<GroomingRecord> GroomingRecords { get; set; } = new List<GroomingRecord>();
}

public class Appointment
{
    public int Id { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;               // each appointment -> ONE pet

    public int PetOwnerId { get; set; }
    public PetOwner PetOwner { get; set; } = null!;

    public int GroomerId { get; set; }
    public Groomer Groomer { get; set; } = null!;       // each appointment -> ONE groomer

    public int ServiceId { get; set; }
    public GroomingService Service { get; set; } = null!;

    public GroomingRecord? GroomingRecord { get; set; } // filled in by the groomer once completed
}

public class GroomingRecord
{
    public int Id { get; set; }                         // auto-generated
    public DateTime GroomingDate { get; set; }
    public string Notes { get; set; } = "";

    public int ServiceId { get; set; }                  // "Service Performed"
    public GroomingService Service { get; set; } = null!;

    public int GroomerId { get; set; }
    public Groomer Groomer { get; set; } = null!;

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
}
