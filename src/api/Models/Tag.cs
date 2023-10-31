using System.ComponentModel.DataAnnotations;
using albumica.Entities;

namespace albumica.Models;

public class TagVM
{
    public TagVM(Tag t)
    {
        Id = t.TagId;
        Name = t.Name;
        Order = t.Order;
    }
    [Required] public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public int Order { get; set; }
}

public class TagLM
{
    [Required] public int Id { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required int Order { get; set; }
    [Required] public required int MediaCount { get; set; }
}

public class TagCM
{
    [Required] public required string Name { get; set; }
    [Required] public required int Order { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        return errorModel.Errors.Count > 0;
    }
}

public class TagUM
{
    [Required] public required string Name { get; set; }
    [Required] public required int Order { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (string.IsNullOrWhiteSpace(Name))
            errorModel.Errors.Add(nameof(Name), "Required");

        return errorModel.Errors.Count > 0;
    }
}