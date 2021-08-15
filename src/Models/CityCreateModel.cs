using System;
using System.Collections.Generic;
using System.Linq;
using albumica.Data;
using albumica.Translations;

namespace albumica.Models
{
    public class CityCreateModel
    {
        public int CountryId { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public Dictionary<string, string>? Validate(ILocations translation, List<City> cities)
        {
            var errors = new Dictionary<string, string>();

            if (CountryId == 0)
                errors.Add(nameof(CountryId), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(nameof(Name), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(DisplayName))
                errors.Add(nameof(DisplayName), translation.ValidationRequired);

            foreach (var city in cities)
            {
                if (city.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                    errors.Add(nameof(Name), translation.ValidationDuplicate);

                if (city.DisplayName.Equals(DisplayName, StringComparison.OrdinalIgnoreCase))
                    errors.Add(nameof(DisplayName), translation.ValidationDuplicate);
            }

            return errors.Any() ? errors : null;
        }
    }
}