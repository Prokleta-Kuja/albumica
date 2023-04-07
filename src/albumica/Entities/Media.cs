namespace albumica.Entities;

public class Media
{
    public int MediaId { get; set; }
    public required string SHA256 { get; set; }
    public required string Original { get; set; }
    public string? Preview { get; set; }
    public bool IsVideo { get; set; }
    public DateTime? Created { get; set; }

    public List<Person>? Persons { get; set; }
    public List<Tag>? Tags { get; set; }
}