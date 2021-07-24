using System;
using System.Collections.Generic;

namespace albumica.Data
{
    public class Person
    {
        public Person(string name)
        {
            Name = name;
        }

        public int PersonId { get; set; }
        public string Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? AiPersonId { get; set; }

        public ICollection<ImagePerson>? Images { get; set; }
    }
}