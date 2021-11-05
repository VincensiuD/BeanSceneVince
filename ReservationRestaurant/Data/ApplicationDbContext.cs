using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationRestaurant.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationOrigin> ReservationOrigins { get; set; }
        public DbSet<ReservationStatus> ReservationStatuses { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Sitting> Sittings { get; set; }
        public DbSet<SittingType> SittingTypes { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Restaurant>()
                .Property(r => r.Name)
                .IsRequired();
            builder.Entity<Restaurant>()
               .Property(r => r.PhoneNumber)
               .IsRequired();
            builder.Entity<Restaurant>()
               .Property(r => r.Address)
               .IsRequired();

            //sitting

            builder.Entity<Sitting>()
               .Property(s => s.Name)
               .IsRequired();
            builder.Entity<Sitting>()
                .Property(s => s.StartTime)
                .IsRequired();
            builder.Entity<Sitting>()
                .Property(s => s.EndTime)
                .IsRequired();
            builder.Entity<Sitting>()
               .Property(s => s.Capacity)
               .IsRequired();

            //Reservation
            builder.Entity<Reservation>()
                .Property(r => r.Guests)
                .IsRequired();
            builder.Entity<Reservation>()
                .Property(r => r.StartTime)
                .IsRequired();

            //person

            builder.Entity<Person>()
               .Property(p => p.FirstName)
               .IsRequired();

            builder.Entity<Person>()
               .Property(p => p.LastName)
               .IsRequired();
            builder.Entity<Person>()
               .Property(p => p.Email)
               .IsRequired();
            builder.Entity<Person>()
              .HasIndex(p => p.Email)
              .IsUnique();
            builder.Entity<Person>()
               .Property(p => p.Phone)
               .IsRequired();

            new DataSeeder(builder);
        }
    }
}
