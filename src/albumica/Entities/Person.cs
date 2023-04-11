namespace albumica.Entities;

public class Person
{
    public int PersonId { get; set; }
    public required string DisplayName { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public List<Media>? Media { get; set; }
}