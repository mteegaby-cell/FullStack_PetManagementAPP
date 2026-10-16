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
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));

        var email = config["SeedAdmin:Email"]!;
        if (await userMgr.FindByEmailAsync(email) is null)
        {
            var admin = new ApplicationUser { UserName = email, Email = email, FullName = "System Admin", EmailConfirmed = true };
            var result = await userMgr.CreateAsync(admin, config["SeedAdmin:Password"]!);
            if (result.Succeeded) await userMgr.AddToRoleAsync(admin, Roles.Admin);
        }

        if (!await db.GroomingServices.AnyAsync())
        {
            db.GroomingServices.AddRange(
                new GroomingService { ServiceName = "Full Groom", Description = "Bath, dry, trim, nails and ears", DurationMinutes = 90, Price = 450 },
                new GroomingService { ServiceName = "Bath & Dry", Description = "Shampoo, conditioner and blow dry", DurationMinutes = 45, Price = 220 },
                new GroomingService { ServiceName = "Nail Trimming", Description = "Nail clip and file", DurationMinutes = 15, Price = 80 },
                new GroomingService { ServiceName = "Flea Treatment", Description = "Medicated flea bath", DurationMinutes = 60, Price = 300 });
            await db.SaveChangesAsync();
        }
    }
}
