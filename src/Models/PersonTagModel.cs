using System;
using albumica.Data;

namespace albumica.Models
{
    public class PersonTagModel : IComparable<PersonTagModel>
    {
        public PersonTagModel(Person person, int imageCount)
        {
            PersonId = person.PersonId;
            Name = person.Name;
            FirstName = person.FirstName;
            LastName = person.LastName;
            DateOfBirth = person.DateOfBirth;
            AiPersonId = person.AiPersonId;
            ImageCount = imageCount;
        }
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? AiPersonId { get; set; }
        public int ImageCount { get; set; }

        public int CompareTo(PersonTagModel? other)
        {
            if (other == null)
                return 1;

            var count = ImageCount.CompareTo(other.ImageCount);
            if (count == 0)
                return Name.CompareTo(other.Name);

            return -count;
        }
    }
}