using System.ComponentModel.DataAnnotations;

namespace e_commerce.Model;

public class User
{
    [Key]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Rol { get; set; }
}