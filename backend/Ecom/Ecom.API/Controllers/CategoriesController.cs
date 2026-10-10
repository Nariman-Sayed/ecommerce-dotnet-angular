using Ecom.API.Helper;
using Ecom.Application;
using Ecom.Core.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category is null)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CategoryDTO categoryDTO)
    {
        await _categoryService.AddAsync(categoryDTO);
        return Ok(new ResponseAPI(200, "Item has been added"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryDTO categoryDTO)
    {
        var updated = await _categoryService.UpdateAsync(id, categoryDTO);
        if (!updated)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        return Ok(new ResponseAPI(200, "Item has been updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        return Ok(new ResponseAPI(200, "Item has been deleted"));
    }
}
