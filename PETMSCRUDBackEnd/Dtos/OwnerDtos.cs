using System.ComponentModel.DataAnnotations;

namespace PETMSCRUD.Dtos;

public record OwnerProfileDto(string OwnerNumber, string Name, string Surname,
    string Email, string PhoneNumber, string HomeAddress);

public record OwnerPetDto(int Id, string PetNumber, string PetName, string Species, string? Breed,
    string Gender, DateOnly DateOfBirth, string? Colour, decimal Weight, string? MedicalNotes);

public record ServiceDto(int Id, string ServiceName, string? Description, int DurationMinutes, decimal Price);

public record OwnerAppointmentDto(int Id, DateOnly AppointmentDate, TimeOnly AppointmentTime,
    string Status, string? Notes, int PetId, string PetName, string ServiceName,
    int DurationMinutes, decimal Price, string? GroomerName);

public record GroomingHistoryDto(int Id, DateOnly GroomingDate, string ServicePerformed,
    string? Notes, string GroomerName);

public class BookAppointmentDto
{
    [Required] public int PetId { get; set; }
    [Required] public int GroomingServiceId { get; set; }
    [Required] public DateOnly AppointmentDate { get; set; }
    [Required] public TimeOnly AppointmentTime { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
