using AutoMapper;
using Ecom.infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers;

/// <summary>
/// Base class that exposes the data context and object mapper to API controllers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// The database context shared with derived controllers.
    /// </summary>
    protected readonly AppDbContext context;

    /// <summary>
    /// The object mapper shared with derived controllers.
    /// </summary>
    protected readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="mapper">The object mapper.</param>
    public BaseController(AppDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }
}
