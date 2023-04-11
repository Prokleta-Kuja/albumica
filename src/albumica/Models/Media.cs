using System.ComponentModel.DataAnnotations;
using albumica.Entities;

namespace albumica.Models;

public class MediaVM
{
    public MediaVM(Media m)
    {
        Id = m.MediaId;
        Original = C.Paths.MediaRequestFor(m.Original);
        Preview = !string.IsNullOrWhiteSpace(m.Preview) ? C.Paths.MediaRequestFor(m.Preview) : null;
        IsVideo = m.IsVideo;
        Created = m.Created;
    }
    [Required] public int Id { get; set; }
    [Required] public string Original { get; set; }
    public string? Preview { get; set; }
    [Required] public bool IsVideo { get; set; }
    public DateTime? Created { get; set; }
}

public class MediaLM
{
    public MediaLM(string original, string? preview)
    {
        Original = C.Paths.MediaRequestFor(original);
        Preview = !string.IsNullOrWhiteSpace(preview) ? C.Paths.MediaRequestFor(preview) : null;
    }
    [Required] public int Id { get; set; }
    [Required] public string Original { get; set; }
    public string? Preview { get; set; }
    [Required] public bool IsVideo { get; set; }
    public DateTime? Created { get; set; }
}