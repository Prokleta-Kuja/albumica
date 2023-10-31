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
        Hidden = m.Hidden;
        Created = m.Created;
        TagIds = m.Tags?.Select(t => t.TagId).ToHashSet() ?? new();
    }
    [Required] public int Id { get; set; }
    [Required] public string Original { get; set; }
    public string? Preview { get; set; }
    [Required] public bool IsVideo { get; set; }
    [Required] public bool Hidden { get; set; }
    public DateTime? Created { get; set; }
    [Required] public HashSet<int> TagIds { get; set; }
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
    [Required] public bool InBasket { get; set; }
    [Required] public bool HasTags { get; set; }
    [Required] public bool Hidden { get; set; }
}

public class MediaUM
{
    [Required] public required bool Hidden { get; set; }
    public DateTime? Created { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();
        return false;

        // if (string.IsNullOrWhiteSpace(Name))
        //     errorModel.Errors.Add(nameof(Name), "Required");

        // return errorModel.Errors.Count > 0;
    }
}