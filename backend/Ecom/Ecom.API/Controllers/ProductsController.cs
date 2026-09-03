using Ecom.API.DTO;
using Ecom.API.Helper;
using Ecom.Core.DTO;
using Ecom.Core.Entities.Product;
using Ecom.infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : BaseController
{
    private readonly IImageManagementServices imageManagementServices;

    public ProductsController(AppDbContext context, IMapper mapper, IImageManagementServices imageManagementServices)
        : base(context, mapper)
    {
        this.imageManagementServices = imageManagementServices;
    }

    private static IEnumerable<UploadedFile> ToUploadedFiles(IFormFileCollection files) =>
        files.Where(f => f.Length > 0).Select(f => new UploadedFile(f.FileName, f.OpenReadStream()));

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await context.Products
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .AsNoTracking()
            .ToListAsync();

        return Ok(mapper.Map<List<ProductDTO>>(products));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await context.Products
            .Include(p => p.Category)
            .Include(p => p.Photos)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        return Ok(mapper.Map<ProductDTO>(product));
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddProductDTO productDTO)
    {
        var product = mapper.Map<Product>(productDTO);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var imagePaths = await imageManagementServices.AddImageAsync(ToUploadedFiles(productDTO.Photo), productDTO.Name);
        var photos = imagePaths.Select(path => new Photo { ImageName = path, ProductId = product.Id }).ToList();

        context.Photos.AddRange(photos);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new ResponseAPI(201, "Product created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductDTO productDTO)
    {
        var product = await context.Products
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        mapper.Map(productDTO, product);

        var oldImageNames = product.Photos.Select(p => p.ImageName).ToList();
        context.Photos.RemoveRange(product.Photos);

        var imagePaths = await imageManagementServices.AddImageAsync(ToUploadedFiles(productDTO.Photo), productDTO.Name);
        var newPhotos = imagePaths.Select(path => new Photo { ImageName = path, ProductId = product.Id }).ToList();
        context.Photos.AddRange(newPhotos);

        await context.SaveChangesAsync();

        // Delete old files from disk only after the database update succeeds
        foreach (var imageName in oldImageNames)
            imageManagementServices.DeleteImageAsync(imageName);

        return Ok(new ResponseAPI(200, "Product updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await context.Products
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        var imageNames = product.Photos.Select(p => p.ImageName).ToList();

        context.Products.Remove(product);
        await context.SaveChangesAsync();

        // Delete files from disk only after the database delete succeeds
        foreach (var imageName in imageNames)
            imageManagementServices.DeleteImageAsync(imageName);

        return Ok(new ResponseAPI(200, "Product deleted"));
    }
}
