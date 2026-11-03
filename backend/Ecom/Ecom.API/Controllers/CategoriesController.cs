using Ecom.API.Helper;
using Ecom.Application;
using Ecom.Core.DTO;
using Ecom.Core.Entities.Product;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers;

/// <summary>
/// Manages product categories.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoriesController"/> class.
    /// </summary>
    /// <param name="categoryService">The category application service.</param>
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <returns>Every category in the catalogue.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Category>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a single category by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the category to retrieve.</param>
    /// <returns>The requested category, or a not-found response when it does not exist.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category is null)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        return Ok(category);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="categoryDTO">The name and description of the category to create.</param>
    /// <returns>A created response whose location points to the new category.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status201Created)]
    public async Task<IActionResult> Add(CategoryDTO categoryDTO)
    {
        var id = await _categoryService.AddAsync(categoryDTO);

        return CreatedAtAction(nameof(GetById), new { id }, new ResponseAPI(201, "Item has been added"));
    }

    /// <summary>
    /// Updates the name and description of an existing category.
    /// </summary>
    /// <param name="id">The identifier of the category to update.</param>
    /// <param name="categoryDTO">The new name and description.</param>
    /// <returns>An OK response when updated, or a not-found response when the category does not exist.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateCategoryDTO categoryDTO)
    {
        var updated = await _categoryService.UpdateAsync(id, categoryDTO);
        if (!updated)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        return Ok(new ResponseAPI(200, "Item has been updated"));
    }

    /// <summary>
    /// Deletes an existing category.
    /// </summary>
    /// <param name="id">The identifier of the category to delete.</param>
    /// <returns>A no-content response when deleted, or a not-found response when the category does not exist.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        return NoContent();
    }
}
