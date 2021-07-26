using System.Collections.Generic;

namespace albumica.Data
{
    public class City
    {
        public City(string name)
        {
            Name = name;
        }

        public int CityId { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }

        public Country? Country { get; set; }
        public ICollection<Location>? Locations { get; set; }
    }
}