using AutoMapper;
using Ecom.API.Helper;
using Ecom.Core.DTO;
using Ecom.Core.Entities.Product;
using Ecom.infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : BaseController
{
    public CategoriesController(AppDbContext context, IMapper mapper) : base(context, mapper)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await context.Categories.AsNoTracking().ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await context.Categories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Add(CategoryDTO categoryDTO)
    {
        var category = mapper.Map<Category>(categoryDTO);
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return Ok(new ResponseAPI(200, "Item has been added"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryDTO categoryDTO)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        mapper.Map(categoryDTO, category);
        await context.SaveChangesAsync();
        return Ok(new ResponseAPI(200, "Item has been updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return NotFound(new ResponseAPI(404, $"Category with id={id} not found"));

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return Ok(new ResponseAPI(200, "Item has been deleted"));
    }
}