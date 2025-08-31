namespace e_commerce.Model.Dtos;

public class UserRegisterDto
{
    public string? ID { get; set; }
    public string? Name { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? Rol { get; set; }
}