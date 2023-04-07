namespace albumica.Entities;

public class Tag
{
    public int TagId { get; set; }
    public required string Name { get; set; }

    public List<Media>? Media { get; set; }
}