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
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<ImagePerson> ImagePersons { get; set; } = null!;
        public DbSet<Person> People { get; set; } = null!;
        public DbSet<Video> Videos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Image>(e =>
            {
                e.HasKey(p => p.ImageId);
                e.HasOne(p => p.Video).WithOne(p => p!.Image!).HasForeignKey<Image>(p => p.VideoId);
            });

            builder.Entity<ImagePerson>(e =>
            {
                e.HasKey(p => new { p.ImageId, p.PersonId });
                e.HasOne(p => p.Person).WithMany(p => p!.Images).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(p => p.Image).WithMany(p => p!.Persons).OnDelete(DeleteBehavior.Cascade);
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