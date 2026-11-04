namespace Ecom.Application;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponseDTO>> GetAllAsync();

    Task<CategoryResponseDTO?> GetByIdAsync(int id);

    Task<CategoryResponseDTO> AddAsync(CategoryDTO category);

    Task<CategoryResponseDTO?> UpdateAsync(int id, UpdateCategoryDTO category);

    Task<bool> DeleteAsync(int id);
}
