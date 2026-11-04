namespace Ecom.Application;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _context;

    public CategoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryResponseDTO>> GetAllAsync()
    {
        var categories = await _context.Categories.AsNoTracking().ToListAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryResponseDTO?> GetByIdAsync(int id)
    {
        var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return category is null ? null : ToDto(category);
    }

    public async Task<CategoryResponseDTO> AddAsync(CategoryDTO category)
    {
        var created = category.ToCategory();
        _context.Categories.Add(created);
        await _context.SaveChangesAsync();
        return ToDto(created);
    }

    public async Task<CategoryResponseDTO?> UpdateAsync(int id, UpdateCategoryDTO category)
    {
        var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
            return null;

        existing.UpdateDetails(category.Name, category.Description);
        await _context.SaveChangesAsync();
        return ToDto(existing);
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

    private static CategoryResponseDTO ToDto(Category category) =>
        new(category.Id, category.Name, category.Description);
}
