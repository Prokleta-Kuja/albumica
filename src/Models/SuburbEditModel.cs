using System;
using System.Collections.Generic;
using System.Linq;
using albumica.Data;
using albumica.Translations;

namespace albumica.Models
{
    public class SuburbEditModel
    {
        public int SuburbId { get; set; }
        public int CityId { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }

        public SuburbEditModel(Suburb s)
        {
            SuburbId = s.SuburbId;
            CityId = s.CityId;
            Name = s.Name;
            DisplayName = s.DisplayName;
        }
        public Dictionary<string, string>? Validate(ILocations translation, List<Suburb> suburbs)
        {
            var errors = new Dictionary<string, string>();

            if (CityId == 0)
                errors.Add(nameof(CityId), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(nameof(Name), translation.ValidationRequired);

            if (string.IsNullOrWhiteSpace(DisplayName))
                errors.Add(nameof(DisplayName), translation.ValidationRequired);

            foreach (var suburb in suburbs)
            {
                if (suburb.CityId != CityId || suburb.SuburbId == SuburbId)
                    continue;

                if (suburb.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                    errors.Add(nameof(Name), translation.ValidationDuplicate);

                if (suburb.DisplayName.Equals(DisplayName, StringComparison.OrdinalIgnoreCase))
                    errors.Add(nameof(DisplayName), translation.ValidationDuplicate);
            }

            return errors.Any() ? errors : null;
        }
    }
}