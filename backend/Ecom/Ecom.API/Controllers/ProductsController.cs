using Ecom.API.DTO;
using Ecom.API.Helper;
using Ecom.Application;
using Ecom.Core.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromForm] AddProductRequest request)
    {
        var id = await _productService.AddAsync(ToCoreAddDto(request), ToImageUploads(request.Photo));
        return CreatedAtAction(nameof(GetById), new { id }, new ResponseAPI(201, "Product created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProductRequest request)
    {
        var updated = await _productService.UpdateAsync(id, ToCoreUpdateDto(request), ToImageUploads(request.Photo));
        if (!updated)
            return NotFound(new ResponseAPI(404, $"Product id={id} not found"));

        return Ok(new ResponseAPI(200, "Product updated"));
    }

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
