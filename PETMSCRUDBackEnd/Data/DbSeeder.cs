using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PETMSCRUD.Models;

namespace  PETMSCRUD.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var config = sp.GetRequiredService<IConfiguration>();
        var db = sp.GetRequiredService<PETMSDBContext>();

        foreach (var role in new[] { Roles.Admin, Roles.Groomer, Roles.PetOwner })
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                var roleResult = await roleMgr.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Could not create role '{role}': " +
                        string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "Configure SeedAdmin:Email and SeedAdmin:Password in user secrets or appsettings before running DbSeeder.");

        var admin = await userMgr.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = "System Admin",
                EmailConfirmed = true
            };
            var result = await userMgr.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Could not create seed admin: " +
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userMgr.IsInRoleAsync(admin, Roles.Admin))
        {
            var roleResult = await userMgr.AddToRoleAsync(admin, Roles.Admin);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    "Could not assign seed admin role: " +
                    string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        // Add only missing services so this seeder can run before or after SeedData.
        var serviceSeeds = new[]
        {
            new GroomingService { ServiceName = "Full Groom", Description = "Bath, dry, trim, nails and ears", DurationMinutes = 90, Price = 450 },
            new GroomingService { ServiceName = "Bath & Dry", Description = "Shampoo, conditioner and blow dry", DurationMinutes = 45, Price = 220 },
            new GroomingService { ServiceName = "Nail Trimming", Description = "Nail clip and file", DurationMinutes = 15, Price = 80 },
            new GroomingService { ServiceName = "Flea Treatment", Description = "Medicated flea bath", DurationMinutes = 60, Price = 300 }
        };

        foreach (var service in serviceSeeds)
        {
            if (!await db.GroomingServices.AnyAsync(s => s.ServiceName == service.ServiceName))
                db.GroomingServices.Add(service);
        }

        await db.SaveChangesAsync();
    }
}
