using System.ComponentModel.DataAnnotations;

namespace albumica.Models;

public class PlainError
{
    public PlainError() { Message = "Validation error"; }
    public PlainError(string message) { Message = message; }
    [Required] public string Message { get; set; }
}

public class ValidationError : PlainError
{
    public ValidationError() { }
    public ValidationError(Dictionary<string, string> errors)
    {
        Errors = errors;
    }
    public ValidationError(string key, string message)
    {
        Errors.Add(key, message);
    }
    public Dictionary<string, string> Errors { get; set; } = new();
}