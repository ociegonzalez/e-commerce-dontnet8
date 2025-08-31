using e_commerce.Model;
using Microsoft.EntityFrameworkCore;

namespace e_commerce.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options){}
    
    public DbSet<Category>  Categories { get; set; }
    public DbSet<Product>  Products { get; set; }
    public DbSet<User>  Users { get; set; }
}