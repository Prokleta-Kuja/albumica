using System.Collections.Generic;

namespace albumica.Data
{
    public class Suburb
    {
        public Suburb(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public int SuburbId { get; set; }
        public int CityId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public City? City { get; set; }
        public ICollection<Location>? Locations { get; set; }
    }
}