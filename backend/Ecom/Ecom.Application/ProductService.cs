namespace Ecom.Application;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;
    private readonly IImageStorage _imageStorage;

    public ProductService(IApplicationDbContext context, IImageStorage imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
    }

    public async Task<IReadOnlyList<ProductDTO>> GetAllAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .AsNoTracking()
            .ToListAsync();

        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDTO?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : ToDto(product);
    }

    public async Task<int> AddAsync(AddProductDTO product, IReadOnlyList<ImageUpload> images)
    {
        var entity = new Product(product.Name, product.Description, product.CategoryId, product.OldPrice, product.NewPrice);

        _context.Products.Add(entity);
        await _context.SaveChangesAsync();

        var imagePaths = await _imageStorage.SaveAsync(images, product.Name);
        foreach (var imagePath in imagePaths)
            entity.AddPhoto(new Photo { ImageName = imagePath, ProductId = entity.Id });

        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDTO product, IReadOnlyList<ImageUpload> images)
    {
        var entity = await _context.Products
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity is null)
            return false;

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

        return true;
    }

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
