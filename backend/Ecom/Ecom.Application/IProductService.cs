namespace Ecom.Application;

public interface IProductService
{
    Task<IReadOnlyList<ProductDTO>> GetAllAsync();

    Task<ProductDTO?> GetByIdAsync(int id);

    Task<int> AddAsync(AddProductDTO product, IReadOnlyList<ImageUpload> images);

    Task<bool> UpdateAsync(int id, UpdateProductDTO product, IReadOnlyList<ImageUpload> images);

    Task<bool> DeleteAsync(int id);
}
