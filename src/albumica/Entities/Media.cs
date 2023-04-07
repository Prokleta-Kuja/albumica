namespace albumica.Entities;

public class Media
{
    public int MediaId { get; set; }
    public required string SHA256 { get; set; }
    public required string Path { get; set; }
    public DateTime? Created { get; set; }

    public Image? Image { get; set; }
    public Video? Video { get; set; }
    public List<Person>? Persons { get; set; }
    public List<Tag>? Tags { get; set; }
}