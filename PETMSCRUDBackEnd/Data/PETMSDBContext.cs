using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PETMSCRUD.Models;

namespace PETMSCRUD.Data;

    public class PETMSDBContext : IdentityDbContext<ApplicationUser>
    {
        public PETMSDBContext(DbContextOptions<PETMSDBContext> options) : base(options) { }

        public DbSet<PetOwner> PetOwners => Set<PetOwner>();
        public DbSet<Pet> Pets => Set<Pet>();
        public DbSet<Groomer> Groomers => Set<Groomer>();
        public DbSet<GroomingService> GroomingServices => Set<GroomingService>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<GroomingRecord> GroomingRecords => Set<GroomingRecord>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);   // required: creates the Identity tables

            // Unique business numbers
            b.Entity<PetOwner>().HasIndex(x => x.OwnerNumber).IsUnique();
            b.Entity<PetOwner>().HasIndex(x => x.Email).IsUnique();
            b.Entity<Groomer>().HasIndex(x => x.StaffNumber).IsUnique();
            b.Entity<Groomer>().HasIndex(x => x.Email).IsUnique();
            b.Entity<Pet>().HasIndex(x => x.PetNumber).IsUnique();


            b.Entity<Pet>().Property(x => x.Weight).HasPrecision(6, 2);
            b.Entity<GroomingService>().Property(x => x.Price).HasPrecision(10, 2);

            // ---- Store enums as readable strings ----
            b.Entity<Pet>().Property(x => x.Gender).HasConversion<string>().HasMaxLength(10);
            b.Entity<GroomingService>().Property(x => x.Status).HasConversion<string>().HasMaxLength(10);
            b.Entity<Appointment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            // Identity user <-> PetOwner / Groomer (one login per person)
            b.Entity<PetOwner>().HasOne(o => o.User).WithMany()
                .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<PetOwner>().HasIndex(o => o.UserId).IsUnique();
            b.Entity<Groomer>().HasOne(g => g.User).WithMany()
                .HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Groomer>().HasIndex(g => g.UserId).IsUnique();

            // One PetOwner -> Many Pets
            b.Entity<Pet>().HasOne(p => p.Owner).WithMany(o => o.Pets)
                .HasForeignKey(p => p.PetOwnerId).OnDelete(DeleteBehavior.Restrict);

            // One Pet -> Many Appointments
            b.Entity<Appointment>().HasOne(a => a.Pet).WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PetId).OnDelete(DeleteBehavior.Restrict);

            // One PetOwner -> Many Appointments (Restrict avoids SQL Server "multiple cascade paths")
            b.Entity<Appointment>().HasOne(a => a.PetOwner).WithMany(o => o.Appointments)
                .HasForeignKey(a => a.PetOwnerId).OnDelete(DeleteBehavior.Restrict);

            // One Groomer -> Many Appointments (groomer optional until admin assigns one)
            b.Entity<Appointment>().HasOne(a => a.Groomer).WithMany(g => g.Appointments)
                .HasForeignKey(a => a.GroomerId).OnDelete(DeleteBehavior.Restrict);

            // One GroomingService -> Many Appointments
            b.Entity<Appointment>().HasOne(a => a.GroomingService).WithMany(s => s.Appointments)
                .HasForeignKey(a => a.GroomingServiceId).OnDelete(DeleteBehavior.Restrict);

            // One Pet -> Many GroomingRecords
            b.Entity<GroomingRecord>().HasOne(r => r.Pet).WithMany(p => p.GroomingRecords)
                .HasForeignKey(r => r.PetId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<GroomingRecord>().HasOne(r => r.Groomer).WithMany()
                .HasForeignKey(r => r.GroomerId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<GroomingRecord>().HasOne(r => r.GroomingService).WithMany()
                .HasForeignKey(r => r.GroomingServiceId).OnDelete(DeleteBehavior.Restrict);

            // Appointment 1 -> 0..1 Grooming Record
            b.Entity<GroomingRecord>().HasOne(r => r.Appointment).WithOne(a => a.GroomingRecord)
                .HasForeignKey<GroomingRecord>(r => r.AppointmentId).OnDelete(DeleteBehavior.Restrict);

            // ---- Link domain profiles to Identity accounts ----
            b.Entity<PetOwner>().HasOne(o => o.User).WithMany()
                .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            b.Entity<Groomer>().HasOne(g => g.User).WithMany()
                .HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Restrict);
    }
    }
