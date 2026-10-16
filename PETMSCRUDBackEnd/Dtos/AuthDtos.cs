using System.ComponentModel.DataAnnotations;

namespace PETMSCRUD.Dtos;

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class RegisterDto
{
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    [Required, MaxLength(50)] public string Surname { get; set; } = "";
    [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"^\+?[0-9\s\-]{10,15}$", ErrorMessage = "Enter a valid phone number.")]
    public string PhoneNumber { get; set; } = "";
    [Required, MaxLength(200)] public string HomeAddress { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
}

public record AuthResponseDto(string Token, DateTime ExpiresAt, string DisplayName, string Email, List<string> Roles);
