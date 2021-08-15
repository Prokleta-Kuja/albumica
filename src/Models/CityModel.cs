using System;
using albumica.Data;

namespace albumica.Models
{
    public class CityModel
    {
        public CityModel()
        {
            Name = null!;
            CountryName = null!;
        }
        public CityModel(City c, int suburbCount = 0)
        {
            CityId = c.CityId;
            CountryId = c.CountryId;
            Name = c.Name;
            DisplayName = c.DisplayName;
            CountryName = c.Country?.Name ?? throw new ArgumentNullException(nameof(c.Country));
            SuburbCount = suburbCount;
        }
        public int CityId { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public string CountryName { get; set; }
        public int SuburbCount { get; set; }
    }
}