using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace albumica.Data
{
    public class AppDbContext : DbContext, IDataProtectionKeyContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
        public DbSet<City> Cities { get; set; } = null!;
        public DbSet<Country> Countries { get; set; } = null!;
        public DbSet<Suburb> Suburbs { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<ImagePerson> ImagePersons { get; set; } = null!;
        public DbSet<ImageTag> ImageTags { get; set; } = null!;
        public DbSet<Location> Locations { get; set; } = null!;
        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Video> Videos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<City>(e =>
            {
                e.HasKey(p => p.CityId);
                e.HasOne(p => p.Country).WithMany(p => p!.Cities);
            });

            builder.Entity<Country>(e =>
            {
                e.HasKey(p => p.CountryId);
            });

            builder.Entity<Suburb>(e =>
            {
                e.HasKey(p => p.SuburbId);
                e.HasOne(p => p.City).WithMany(p => p!.Suburbs);
            });

            builder.Entity<Tag>(e =>
            {
                e.HasKey(p => p.TagId);
            });

            builder.Entity<Image>(e =>
            {
                e.HasKey(p => p.ImageId);
                e.HasOne(p => p.Video).WithOne(p => p!.Image!).HasForeignKey<Image>(p => p.VideoId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(p => p.Location).WithOne(p => p!.Image!).HasForeignKey<Image>(p => p.LocationId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ImagePerson>(e =>
            {
                e.HasKey(p => new { p.ImageId, p.PersonId });
                e.HasOne(p => p.Person).WithMany(p => p!.Images).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(p => p.Image).WithMany(p => p!.Persons).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ImageTag>(e =>
            {
                e.HasKey(p => new { p.ImageId, p.TagId });
                e.HasOne(p => p.Tag).WithMany(p => p!.Images).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(p => p.Image).WithMany(p => p!.Tags).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Location>(e =>
            {
                e.HasKey(p => p.LocationId);
                e.HasOne(p => p.Country).WithMany(p => p!.Locations).HasForeignKey(p => p.CountryId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(p => p.City).WithMany(p => p!.Locations).HasForeignKey(p => p.CityId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(p => p.Suburb).WithMany(p => p!.Locations).HasForeignKey(p => p.SuburbId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Person>(e =>
            {
                e.HasKey(p => p.PersonId);
            });

            builder.Entity<Video>(e =>
            {
                e.HasKey(p => p.VideoId);
            });


            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var dtProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var property in dtProperties)
                    builder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeToBinaryConverter());

                var decProperties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

                foreach (var property in decProperties)
                    builder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion<double>();
            }
        }
        public async Task ProvisionDemoAsync()
        {
            await SaveChangesAsync();
        }
    }
}