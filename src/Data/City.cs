using System.Collections.Generic;

namespace albumica.Data
{
    public class City
    {
        public City(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public int CityId { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public Country? Country { get; set; }
        public ICollection<Suburb>? Suburbs { get; set; }
        public ICollection<Location>? Locations { get; set; }
    }
}