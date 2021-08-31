using System;
using System.Collections.Generic;
using System.Linq;
using albumica.Data;
using albumica.Translations;

namespace albumica.Models
{
    public class PersonEditModel
    {
        public int PersonId { get; set; }
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public PersonEditModel(Person p)
        {
            Name = p.Name;
            FirstName = p.FirstName;
            LastName = p.LastName;
            DateOfBirth = p.DateOfBirth;
        }
        public Dictionary<string, string>? Validate(IPeople translation, HashSet<string> names)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(nameof(Name), translation.ValidationRequired);
            else if (names.Contains(Name.ToUpper()))
                errors.Add(nameof(Name), translation.ValidationDuplicate);

            return errors.Any() ? errors : null;
        }
    }
}