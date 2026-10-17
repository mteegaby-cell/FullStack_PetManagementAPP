using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PETMSCRUD.Data;
using PETMSCRUD.Models;

namespace PETMSCRUD.Models;

/// Runs at startup: applies migrations, creates roles, and adds demo data once.
/// Demo logins (password for all: Pass@1234):
///   admin@pawscare.com, groomer1@pawscare.com, groomer2@pawscare.com,
///   owner1@pawscare.com, owner2@pawscare.com
public static class SeedData
{
    public const string DemoPassword = "Pass@1234";

    public static async Task InitializeAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<PETMSDBContext>();
        var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        // Ensure role names exist before creating or assigning demo users.
        foreach (var roleName in new[] { "Admin", "Groomer", "PetOwner" })
        {
            if (!await roles.RoleExistsAsync(roleName))
            {
                var roleResult = await roles.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Could not create role '{roleName}': " +
                        string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        async Task<ApplicationUser> EnsureUser(string email, string role)
        {
            var u = await users.FindByEmailAsync(email);
            if (u == null)
            {
                u = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var res = await users.CreateAsync(u, DemoPassword);
                if (!res.Succeeded)
                    throw new InvalidOperationException(
                        $"Could not create user '{email}': " +
                        string.Join("; ", res.Errors.Select(e => e.Description)));
            }

            if (!await users.IsInRoleAsync(u, role))
            {
                var roleResult = await users.AddToRoleAsync(u, role);
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        $"Could not assign role '{role}' to '{email}': " +
                        string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }

            return u;
        }

        await EnsureUser("admin@pawscare.com", "Admin");

        // Seed demo users and entities idempotently. Do not return early just because
        // owners already exist: the other tables may still need their demo records.
        var g1User = await EnsureUser("groomer1@pawscare.com", "Groomer");
        var g2User = await EnsureUser("groomer2@pawscare.com", "Groomer");
        var o1User = await EnsureUser("owner1@pawscare.com", "PetOwner");
        var o2User = await EnsureUser("owner2@pawscare.com", "PetOwner");

        var g1 = await db.Groomers.SingleOrDefaultAsync(g => g.StaffNumber == "GRM001");
        if (g1 == null)
        {
            g1 = new Groomer { StaffNumber = "GRM001", Name = "Sipho", Surname = "Dlamini", Email = "groomer1@pawscare.com", PhoneNumber = "0821112222", Specialization = "Dog Grooming", UserId = g1User.Id };
            db.Groomers.Add(g1);
        }

        var g2 = await db.Groomers.SingleOrDefaultAsync(g => g.StaffNumber == "GRM002");
        if (g2 == null)
        {
            g2 = new Groomer { StaffNumber = "GRM002", Name = "Anna", Surname = "van Wyk", Email = "groomer2@pawscare.com", PhoneNumber = "0833334444", Specialization = "Cat Grooming", UserId = g2User.Id };
            db.Groomers.Add(g2);
        }

        var full = await db.GroomingServices.SingleOrDefaultAsync(s => s.ServiceName == "Full Groom");
        if (full == null)
        {
            full = new GroomingService { ServiceName = "Full Groom", Description = "Bath, dry, haircut, nails and ears.", DurationMinutes = 90, Price = 450 };
            db.GroomingServices.Add(full);
        }
        var bath = await db.GroomingServices.SingleOrDefaultAsync(s => s.ServiceName == "Bath & Dry");
        if (bath == null)
        {
            bath = new GroomingService { ServiceName = "Bath & Dry", Description = "Shampoo, conditioner and blow dry.", DurationMinutes = 45, Price = 200 };
            db.GroomingServices.Add(bath);
        }
        var nails = await db.GroomingServices.SingleOrDefaultAsync(s => s.ServiceName == "Nail Trimming");
        if (nails == null)
        {
            nails = new GroomingService { ServiceName = "Nail Trimming", Description = "Safe nail trim and file.", DurationMinutes = 20, Price = 80 };
            db.GroomingServices.Add(nails);
        }
        var hair = await db.GroomingServices.SingleOrDefaultAsync(s => s.ServiceName == "Hair Trimming");
        if (hair == null)
        {
            hair = new GroomingService { ServiceName = "Hair Trimming", Description = "Tidy trim to keep the coat neat.", DurationMinutes = 40, Price = 150 };
            db.GroomingServices.Add(hair);
        }
        var ears = await db.GroomingServices.SingleOrDefaultAsync(s => s.ServiceName == "Ear Cleaning");
        if (ears == null)
        {
            ears = new GroomingService { ServiceName = "Ear Cleaning", Description = "Gentle ear cleaning.", DurationMinutes = 20, Price = 90 };
            db.GroomingServices.Add(ears);
        }
        var flea = await db.GroomingServices.SingleOrDefaultAsync(s => s.ServiceName == "Flea Treatment");
        if (flea == null)
        {
            flea = new GroomingService { ServiceName = "Flea Treatment", Description = "Flea bath and treatment.", DurationMinutes = 30, Price = 180 };
            db.GroomingServices.Add(flea);
        }
        var shed = await db.GroomingServices.SingleOrDefaultAsync(s => s.ServiceName == "De-shedding");
        if (shed == null)
        {
            shed = new GroomingService { ServiceName = "De-shedding", Description = "Deep brushing to reduce shedding.", DurationMinutes = 60, Price = 250 };
            db.GroomingServices.Add(shed);
        }

        var o1 = await db.PetOwners.SingleOrDefaultAsync(o => o.OwnerNumber == "OWN0001");
        if (o1 == null)
        {
            o1 = new PetOwner { OwnerNumber = "OWN0001", Name = "Thandi", Surname = "Mokoena", Email = "owner1@pawscare.com", PhoneNumber = "0715551111", HomeAddress = "12 Beach Road, Gqeberha", UserId = o1User.Id };
            db.PetOwners.Add(o1);
        }
        var o2 = await db.PetOwners.SingleOrDefaultAsync(o => o.OwnerNumber == "OWN0002");
        if (o2 == null)
        {
            o2 = new PetOwner { OwnerNumber = "OWN0002", Name = "Liam", Surname = "Naidoo", Email = "owner2@pawscare.com", PhoneNumber = "0726662222", HomeAddress = "45 Main Street, Cape Town", UserId = o2User.Id };
            db.PetOwners.Add(o2);
        }

        // Save parent records before creating pets and appointments so existing rows
        // can be reused safely on subsequent application starts.
        await db.SaveChangesAsync();

        var bella = await db.Pets.SingleOrDefaultAsync(p => p.PetNumber == "PET0001");
        if (bella == null)
        {
            bella = new Pet { PetNumber = "PET0001", PetName = "Bella", Species = "Dog", Breed = "Labrador", Gender = Gender.Female, DateOfBirth = new DateOnly(2020, 3, 14), Colour = "Golden", Weight = 28.5m, MedicalNotes = "Allergic to oatmeal shampoo.", Owner = o1 };
            db.Pets.Add(bella);
        }
        var milo = await db.Pets.SingleOrDefaultAsync(p => p.PetNumber == "PET0002");
        if (milo == null)
        {
            milo = new Pet { PetNumber = "PET0002", PetName = "Milo", Species = "Cat", Breed = "Siamese", Gender = Gender.Male, DateOfBirth = new DateOnly(2021, 7, 2), Colour = "Cream", Weight = 4.2m, Owner = o1 };
            db.Pets.Add(milo);
        }
        var rocky = await db.Pets.SingleOrDefaultAsync(p => p.PetNumber == "PET0003");
        if (rocky == null)
        {
            rocky = new Pet { PetNumber = "PET0003", PetName = "Rocky", Species = "Dog", Breed = "Beagle", Gender = Gender.Male, DateOfBirth = new DateOnly(2019, 11, 20), Colour = "Tricolour", Weight = 11.8m, MedicalNotes = "Sensitive ears.", Owner = o2 };
            db.Pets.Add(rocky);
        }
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);

        Appointment A(Pet p, PetOwner o, Groomer? g, GroomingService s, int dayOffset, int h, int m, AppointmentStatus st, string? notes = null) =>
            new() { Pet = p, PetOwner = o, Groomer = g, GroomingService = s, AppointmentDate = today.AddDays(dayOffset), AppointmentTime = new TimeOnly(h, m), Status = st, Notes = notes };

        if (!await db.Appointments.AnyAsync())
        {
            db.Appointments.AddRange(
                A(bella, o1, g1, full, -30, 9, 0, AppointmentStatus.Completed),
                A(bella, o1, g1, nails, -10, 11, 0, AppointmentStatus.Completed),
                A(bella, o1, g1, bath, 3, 10, 0, AppointmentStatus.Scheduled, "Please use hypoallergenic shampoo."),
                A(milo, o1, g2, shed, -14, 13, 30, AppointmentStatus.Completed),
                A(milo, o1, null, ears, 7, 14, 0, AppointmentStatus.Scheduled),
                A(milo, o1, g2, hair, -5, 15, 0, AppointmentStatus.Cancelled),
                A(rocky, o2, g1, flea, 1, 9, 30, AppointmentStatus.Scheduled));
        }

        if (!await db.GroomingRecords.AnyAsync())
        {
            GroomingRecord R(Pet p, Groomer g, GroomingService s, int dayOffset, string notes) =>
                new() { Pet = p, Groomer = g, GroomingService = s, GroomingDate = today.AddDays(dayOffset), Notes = notes };

            db.GroomingRecords.AddRange(
                R(bella, g1, full, -30, "Coat in good condition. Calm throughout."),
                R(bella, g1, nails, -10, "Nails trimmed. Slight nervousness with the clippers."),
                R(milo, g2, shed, -14, "Lots of loose undercoat removed. Tolerated brushing well."));
        }

        await db.SaveChangesAsync();
    }
}
