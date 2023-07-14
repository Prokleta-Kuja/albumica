namespace albumica.Entities;

public class Media
{
    public int MediaId { get; set; }
    public required string SHA256 { get; set; }
    public required string Import { get; set; }
    public required string Original { get; set; }
    public string? Preview { get; set; }
    public bool IsVideo { get; set; }
    public bool Hidden { get; set; }
    public DateTime? Created { get; set; }

    public List<Tag>? Tags { get; set; }
    public List<User>? Users { get; set; }
}