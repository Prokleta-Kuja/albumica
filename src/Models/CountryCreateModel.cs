using System.Collections.Generic;
using System.Linq;
using albumica.Data;
using albumica.Translations;

namespace albumica.Models
{
    public class CountryCreateModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? DisplayName { get; set; }
        public Dictionary<string, string>? Validate(ILocations translation, HashSet<string> names, HashSet<string> displayNames)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(nameof(Name), translation.ValidationRequired);
            else if (names.Contains(Name.ToUpper()))
                errors.Add(nameof(Name), translation.ValidationDuplicate);

            if (string.IsNullOrWhiteSpace(DisplayName))
                errors.Add(nameof(DisplayName), translation.ValidationRequired);
            else if (displayNames.Contains(DisplayName.ToUpper()))
                errors.Add(nameof(DisplayName), translation.ValidationDuplicate);

            if (string.IsNullOrWhiteSpace(Code))
                errors.Add(nameof(Code), translation.ValidationRequired);


            return errors.Any() ? errors : null;
        }
    }
}