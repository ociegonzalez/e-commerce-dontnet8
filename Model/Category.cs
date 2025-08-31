using System.ComponentModel.DataAnnotations;

namespace e_commerce.Model;

public class Category
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateTime CreationDate { get; set; }
}