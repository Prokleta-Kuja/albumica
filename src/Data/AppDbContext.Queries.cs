using albumica.Models;
using Microsoft.EntityFrameworkCore;

namespace albumica.Data
{
    public partial class AppDbContext
    {
        public DbSet<CountryModel> CountryWithCounts { get; set; } = null!;

        private void AddQueries(ModelBuilder builder)
        {
            builder.Entity<CountryModel>().HasNoKey().ToSqlQuery(@"
SELECT
    co.*
    ,(SELECT count(CityId) FROM Cities WHERE CountryId = co.CountryId) CityCount
    ,(SELECT count(SuburbId) FROM Suburbs WHERE CityId = ct.CityId) SuburbCount
FROM Countries co
    LEFT JOIN Cities ct ON co.CountryId = ct.CountryId
ORDER BY co.Name");
        }
    }
}