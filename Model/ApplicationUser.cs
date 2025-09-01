using Microsoft.AspNetCore.Identity;

namespace e_commerce.Model;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }
}