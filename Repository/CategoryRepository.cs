using e_commerce.Data;
using e_commerce.Model;
using e_commerce.Repository.IRepository;

namespace e_commerce.Repository;

public class CategoryRepository: ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public ICollection<Category> GetCategories()
    {
        return _db.Categories.OrderBy(c => c.Name).ToList();
    }

    public Category? GetCategory(int id)
    {
        return _db.Categories.FirstOrDefault(c => c.Id == id);
    }

    public bool CategoryExists(int id)
    {
        return _db.Categories.Any(c => c.Id == id);
    }

    public bool CategoryExists(string name)
    {
        return _db.Categories.Any(c => c.Name.ToLower().Trim() == name.ToLower().Trim());
    }

    public bool CreateCategory(Category category)
    {
        category.CreationDate = DateTime.Now;
        _db.Categories.Add(category);
        return Save();
    }

    public bool UpdateCategory(Category category)
    {
        category.CreationDate = DateTime.Now;
        _db.Categories.Update(category);
        return Save();
    }

    public bool DeleteCategory(Category category)
    {
        _db.Categories.Remove(category);
        return Save();
    }

    public bool Save()
    {
        return _db.SaveChanges() > 0;
    }
}