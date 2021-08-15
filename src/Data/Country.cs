using System.Collections.Generic;

namespace albumica.Data
{
    public class Country
    {
        public Country(string name, string code, string displayName)
        {
            Name = name;
            Code = code;
            DisplayName = displayName;
        }

        public int CountryId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Code { get; set; }

        public ICollection<City>? Cities { get; set; }
        public ICollection<Location>? Locations { get; set; }
    }
}