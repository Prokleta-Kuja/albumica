namespace albumica.Entities;

public class Tag
{
    public int TagId { get; set; }
    public required string Name { get; set; }
    public required string NameNormalized { get; set; }
    public int Order { get; set; }

    public List<Media>? Media { get; set; }
}