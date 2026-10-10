namespace Ecom.Application;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _context;

    public CategoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await _context.Categories.AsNoTracking().ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(CategoryDTO category)
    {
        _context.Categories.Add(new Category(category.Name, category.Description));
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDTO category)
    {
        var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
            return false;

        existing.UpdateDetails(category.Name, category.Description);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
            return false;

        _context.Categories.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
