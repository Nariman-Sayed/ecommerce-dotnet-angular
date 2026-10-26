using Ecom.API.DTO;
using Ecom.API.Helper;
using Ecom.Application;
using Ecom.Core.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers;

/// <summary>
/// Manages catalogue products.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="productService">The product application service.</param>
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves all products.
    /// </summary>
    /// <returns>Every product in the catalogue.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    /// <summary>
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the product to retrieve.</param>
    /// <returns>The requested product, or a not-found response when it does not exist.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product fields and the uploaded photos.</param>
    /// <returns>A created response whose location points to the new product.</returns>
    [HttpPost]
    public async Task<IActionResult> Add([FromForm] AddProductRequest request)
    {
        var id = await _productService.AddAsync(ToCoreAddDto(request), ToImageUploads(request.Photo));
        return CreatedAtAction(nameof(GetById), new { id }, new ResponseAPI(201, "Product created"));
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The identifier of the product to update.</param>
    /// <param name="request">The new product fields and any replacement photos.</param>
    /// <returns>An OK response when updated, or a not-found response when the product does not exist.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductRequest request)
    {
        var updated = await _productService.UpdateAsync(id, ToCoreUpdateDto(request), ToImageUploads(request.Photo));
        if (!updated)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        return Ok(new ResponseAPI(200, "Product updated"));
    }

    /// <summary>
    /// Deletes an existing product.
    /// </summary>
    /// <param name="id">The identifier of the product to delete.</param>
    /// <returns>An OK response when deleted, or a not-found response when the product does not exist.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        return Ok(new ResponseAPI(200, "Product deleted"));
    }

    private static AddProductDTO ToCoreAddDto(AddProductRequest request) => new()
    {
        Name = request.Name,
        Description = request.Description,
        OldPrice = request.OldPrice,
        NewPrice = request.NewPrice,
        CategoryId = request.CategoryId
    };

    private static UpdateProductDTO ToCoreUpdateDto(UpdateProductRequest request) => new()
    {
        Name = request.Name,
        Description = request.Description,
        OldPrice = request.OldPrice,
        NewPrice = request.NewPrice,
        CategoryId = request.CategoryId
    };

    private static IReadOnlyList<ImageUpload> ToImageUploads(IFormFileCollection files) =>
        files.Where(f => f.Length > 0)
            .Select(f => new ImageUpload(f.FileName, f.OpenReadStream()))
            .ToList();
}
