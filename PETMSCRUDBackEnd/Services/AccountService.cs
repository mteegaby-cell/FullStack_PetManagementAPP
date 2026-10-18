using Microsoft.AspNetCore.Identity;
using PETMSCRUD.Models;

namespace PETMSCRUD.Services;

/// <summary>Wraps ASP.NET Identity's UserManager so controllers can register / update / delete login accounts.</summary>
public class AccountService(UserManager<ApplicationUser> users)
{
    public async Task<(ApplicationUser? User, IEnumerable<string> Errors)> CreateAsync(
        string email, string password, string fullName, string role)
    {
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, EmailConfirmed = true };
        var result = await users.CreateAsync(user, password);       // hashes the password
        if (!result.Succeeded) return (null, result.Errors.Select(e => e.Description));
        await users.AddToRoleAsync(user, role);
        return (user, Array.Empty<string>());
    }

    public async Task UpdateAsync(string? userId, string email, string fullName)
    {
        if (userId is null) return;
        var user = await users.FindByIdAsync(userId);
        if (user is null) return;
        user.Email = email; user.UserName = email; user.FullName = fullName;
        await users.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(string? userId)
    {
        if (userId is null) return;
        var user = await users.FindByIdAsync(userId);
        if (user is not null) await users.DeleteAsync(user);
    }
}
