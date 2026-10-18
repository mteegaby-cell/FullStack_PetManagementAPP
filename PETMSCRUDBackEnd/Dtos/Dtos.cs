using System.ComponentModel.DataAnnotations;
using PETMSCRUD.Models;

namespace PETMSCRUD.Dtos;

// ---------- Auth ----------
public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}
public record AuthResponse(string Token, string Email, string FullName, string Role, DateTime ExpiresAt);

// ---------- Pet owners ----------
public class OwnerRequest
{
    [Required, StringLength(20)] public string OwnerNumber { get; set; } = "";
    [Required, StringLength(50)] public string Name { get; set; } = "";
    [Required, StringLength(50)] public string Surname { get; set; } = "";
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"^\+?[0-9\s\-]{10,15}$", ErrorMessage = "Enter a valid phone number (10-15 digits).")]
    public string PhoneNumber { get; set; } = "";
    [Required, StringLength(200)] public string HomeAddress { get; set; } = "";
    /// <summary>Required on create only (creates the login account).</summary>
    [StringLength(100, MinimumLength = 8)] public string? Password { get; set; }
}
public record OwnerResponse(int Id, string OwnerNumber, string Name, string Surname, string Email,
    string PhoneNumber, string HomeAddress, int PetCount);

// ---------- Groomers ----------
public class GroomerRequest
{
    [Required, StringLength(20)] public string StaffNumber { get; set; } = "";
    [Required, StringLength(50)] public string Name { get; set; } = "";
    [Required, StringLength(50)] public string Surname { get; set; } = "";
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"^\+?[0-9\s\-]{10,15}$", ErrorMessage = "Enter a valid phone number (10-15 digits).")]
    public string PhoneNumber { get; set; } = "";
    [Required, StringLength(100)] public string Specialization { get; set; } = "";
    [StringLength(100, MinimumLength = 8)] public string? Password { get; set; }
}
public record GroomerResponse(int Id, string StaffNumber, string Name, string Surname, string Email,
    string PhoneNumber, string Specialization);

// ---------- Pets ----------
public class PetRequest
{
    [Required, StringLength(20)] public string PetNumber { get; set; } = "";
    [Required, StringLength(50)] public string PetName { get; set; } = "";
    [Required, StringLength(30)] public string Species { get; set; } = "";
    [Required, StringLength(50)] public string Breed { get; set; } = "";
    [Required] public Gender Gender { get; set; }
    [Required] public DateOnly DateOfBirth { get; set; }
    [Required, StringLength(30)] public string Colour { get; set; } = "";
    [Range(0.01, 200)] public decimal Weight { get; set; }
    [StringLength(500)] public string? MedicalNotes { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Owner is required.")] public int OwnerId { get; set; }
}
public record PetResponse(int Id, string PetNumber, string PetName, string Species, string Breed, Gender Gender,
    DateOnly DateOfBirth, string Colour, decimal Weight, string? MedicalNotes, int OwnerId, string OwnerName);

// ---------- Grooming services ----------
public class ServiceRequest
{
    [Required, StringLength(100)] public string ServiceName { get; set; } = "";
    [Required, StringLength(300)] public string Description { get; set; } = "";
    [Range(5, 480, ErrorMessage = "Duration must be 5-480 minutes.")] public int DurationMinutes { get; set; }
    [Range(0, 100000)] public decimal Price { get; set; }
    public ServiceStatus Status { get; set; } = ServiceStatus.Active;
}
public record ServiceResponse(int Id, string ServiceName, string Description, int DurationMinutes, decimal Price, ServiceStatus Status);

// ---------- Appointments ----------
public class AppointmentRequest
{
    [Required] public DateOnly AppointmentDate { get; set; }
    [Required] public TimeOnly AppointmentTime { get; set; }
    [Range(1, int.MaxValue)] public int PetId { get; set; }
    [Range(1, int.MaxValue)] public int GroomerId { get; set; }
    [Range(1, int.MaxValue)] public int ServiceId { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    [StringLength(500)] public string? Notes { get; set; }
}
public record AppointmentResponse(int Id, DateOnly AppointmentDate, TimeOnly AppointmentTime,
    int PetId, string PetName, int PetOwnerId, string OwnerName,
    int GroomerId, string GroomerName, int ServiceId, string ServiceName, int DurationMinutes, decimal Price,
    AppointmentStatus Status, string? Notes);
