namespace Ecom.Application;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task AddAsync(CategoryDTO category);

    Task<bool> UpdateAsync(int id, UpdateCategoryDTO category);

    Task<bool> DeleteAsync(int id);
}
