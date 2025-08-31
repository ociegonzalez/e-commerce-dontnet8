using System.ComponentModel.DataAnnotations;

namespace e_commerce.Model.Dtos;

public class UserLoginDto
{
    [Required(ErrorMessage = "El campo username es requerido")]
    public string? Username { get; set; }
    [Required(ErrorMessage = "El campo password es requerido")]
    public string? Password { get; set; }
}