namespace albumica.Entities;

public class Media
{
    public int MediaId { get; set; }
    public required string Hash { get; set; }
    public bool IsVideo { get; set; }
    public DateTime Created { get; set; }
}