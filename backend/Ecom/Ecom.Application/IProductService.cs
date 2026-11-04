namespace Ecom.Application;

public interface IProductService
{
    Task<IReadOnlyList<ProductDTO>> GetAllAsync();

    Task<ProductDTO?> GetByIdAsync(int id);

    Task<ProductDTO> AddAsync(AddProductDTO product, IReadOnlyList<ImageUpload> images);

    Task<ProductDTO?> UpdateAsync(int id, UpdateProductDTO product, IReadOnlyList<ImageUpload> images);

    Task<bool> DeleteAsync(int id);
}
