using System;
using albumica.Data;

namespace albumica.Models
{
    public class PersonModel
    {
        public PersonModel()
        {
            Name = null!;
        }
        public PersonModel(Person p, int imageCount = 0)
        {
            PersonId = p.PersonId;
            Name = p.Name;
            FirstName = p.FirstName;
            LastName = p.LastName;
            DateOfBirth = p.DateOfBirth;
            AiPersonId = p.AiPersonId;
            ImageCount = imageCount;
        }
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? AiPersonId { get; set; }
        public int ImageCount { get; set; }
    }
}