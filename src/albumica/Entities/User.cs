using System.Diagnostics;

namespace albumica.Entities;

[DebuggerDisplay("Name={Name} Admin={IsAdmin}")]
public class User
{
    public int UserId { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public bool IsAdmin { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime? LastUpload { get; set; }
    public DateTime? Disabled { get; set; }
}