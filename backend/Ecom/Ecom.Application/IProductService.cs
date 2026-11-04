using Ecom.Core.DTO;

namespace Ecom.Application;

/// <summary>
/// Application service for catalogue products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Returns a page of products matching the supplied query parameters.
    /// </summary>
    /// <param name="productParams">The query parameters.</param>
    /// <returns>A page of products.</returns>
    Task<Pagination<ProductDTO>> GetAllAsync(ProductParams productParams);

    /// <summary>
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the product to retrieve.</param>
    /// <returns>The requested product, or <c>null</c> when it does not exist.</returns>
    Task<ProductDTO?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product fields.</param>
    /// <param name="images">The uploaded images.</param>
    /// <returns>The created product.</returns>
    Task<ProductDTO> AddAsync(AddProductDTO product, IReadOnlyList<ImageUpload> images);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The identifier of the product to update.</param>
    /// <param name="product">The new product fields.</param>
    /// <param name="images">Any replacement photos.</param>
    /// <returns>The updated product, or <c>null</c> when it does not exist.</returns>
    Task<ProductDTO?> UpdateAsync(int id, UpdateProductDTO product, IReadOnlyList<ImageUpload> images);

    /// <summary>
    /// Deletes an existing product.
    /// </summary>
    /// <param name="id">The identifier of the product to delete.</param>
    /// <returns><c>true</c> when deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(int id);
}