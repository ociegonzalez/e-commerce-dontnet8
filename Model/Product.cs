using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace e_commerce.Model;

public class Product
{
    [Key]
    public int ProductId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string? ImageUrlLocal { get; set; }
    
    [Required]
    public string SKU { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public int Stock { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? UpdateDate { get; set; } = null;
    
    //Relacion con el modelo categori
    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
}