using Ecom.Application.Extensions;
using Ecom.Core.DTO;
using Ecom.Core.Entities.Product;

namespace Ecom.Application;

/// <summary>
/// Application service for catalogue products.
/// </summary>
public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;
    private readonly IImageStorage _imageStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="imageStorage">The image storage port.</param>
    public ProductService(IApplicationDbContext context, IImageStorage imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
    }

    /// <summary>
    /// Returns a page of products matching the supplied query parameters.
    /// </summary>
    /// <param name="productParams">The query parameters.</param>
    /// <returns>A page of products.</returns>
    public async Task<Pagination<ProductDTO>> GetAllAsync(ProductParams productParams)
    {
        productParams ??= new ProductParams();

        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .AsNoTracking()
            .ApplyFilter(productParams.CategoryId)
            .ApplySearch(productParams.Search)
            .ApplySort(productParams.Sort);

        var count = await query.CountAsync();

        var products = await query
            .ApplyPaging(productParams.PageIndex, productParams.PageSize)
            .ToListAsync();

        var data = products.Select(ToDto).ToList();
        return new Pagination<ProductDTO>(productParams.PageIndex, productParams.PageSize, count, data);
    }

    /// <summary>
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the product to retrieve.</param>
    /// <returns>The requested product, or <c>null</c> when it does not exist.</returns>
    public async Task<ProductDTO?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : ToDto(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product fields.</param>
    /// <param name="images">The uploaded images.</param>
    /// <returns>The created product.</returns>
    public async Task<ProductDTO> AddAsync(AddProductDTO product, IReadOnlyList<ImageUpload> images)
    {
        var entity = product.ToProduct();

        _context.Products.Add(entity);
        await _context.SaveChangesAsync();

        var imagePaths = await _imageStorage.SaveAsync(images, product.Name);
        foreach (var imagePath in imagePaths)
            entity.AddPhoto(new Photo { ImageName = imagePath, ProductId = entity.Id });

        await _context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id)
            ?? throw new InvalidOperationException($"Product id={entity.Id} was not found after creation.");
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The identifier of the product to update.</param>
    /// <param name="product">The new product fields.</param>
    /// <param name="images">Any replacement photos.</param>
    /// <returns>The updated product, or <c>null</c> when it does not exist.</returns>
    public async Task<ProductDTO?> UpdateAsync(int id, UpdateProductDTO product, IReadOnlyList<ImageUpload> images)
    {
        var entity = await _context.Products
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity is null)
            return null;

        entity.UpdateDetails(product.Name, product.Description)
            .UpdatePricing(product.OldPrice, product.NewPrice);

        var oldImageNames = entity.Photos.Select(p => p.ImageName).ToList();
        _context.Photos.RemoveRange(entity.Photos);

        var imagePaths = await _imageStorage.SaveAsync(images, product.Name);
        foreach (var imagePath in imagePaths)
            entity.AddPhoto(new Photo { ImageName = imagePath, ProductId = entity.Id });

        await _context.SaveChangesAsync();

        foreach (var imageName in oldImageNames)
            _imageStorage.Delete(imageName);

        return await GetByIdAsync(id);
    }

    /// <summary>
    /// Deletes an existing product.
    /// </summary>
    /// <param name="id">The identifier of the product to delete.</param>
    /// <returns><c>true</c> when deleted; otherwise <c>false</c>.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Products
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity is null)
            return false;

        var imageNames = entity.Photos.Select(p => p.ImageName).ToList();

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync();

        foreach (var imageName in imageNames)
            _imageStorage.Delete(imageName);

        return true;
    }

    private static ProductDTO ToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        OldPrice = product.OldPrice,
        NewPrice = product.NewPrice,
        CategoryName = product.Category.Name,
        Photos = product.Photos
            .Select(p => new PhotoDTO { ImageName = p.ImageName, ProductId = p.ProductId })
            .ToList()
    };
}