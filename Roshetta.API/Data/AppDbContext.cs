using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Roshetta.API.Models;

namespace Roshetta.API.Data
{
    public class AppDbContext:IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Booking>()
                .HasIndex(b => b.DaySlotId)
                .IsUnique()
                .HasFilter($"[Status] <> {(int)BookingStatus.Cancelled}");

            builder.Entity<Doctor>()
                .HasOne(d => d.Specialization)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);


            foreach (var fk in builder.Model.GetEntityTypes()        // 1
             .Where(e => e.ClrType == typeof(Booking))               // 2
             .SelectMany(e => e.GetForeignKeys()))                   // 3
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;         // 4
            }
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<DaySlot> DaySlots { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<WorkingDay> WorkingDays { get; set; }

    }
}
