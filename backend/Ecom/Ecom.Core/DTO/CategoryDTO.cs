using Ecom.Core.Entities.Product;

namespace Ecom.Core.DTO;

public record CategoryDTO(string Name, string Description)
{
    public Category ToCategory() => new(Name, Description);
}

public record UpdateCategoryDTO(string Name, string Description);
