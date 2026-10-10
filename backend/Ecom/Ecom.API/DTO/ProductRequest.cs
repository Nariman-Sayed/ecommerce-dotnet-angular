using Microsoft.AspNetCore.Http;

namespace Ecom.API.DTO;

public record AddProductRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public int CategoryId { get; set; }
    public required IFormFileCollection Photo { get; set; }
}

public record UpdateProductRequest : AddProductRequest;
