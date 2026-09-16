using AutoMapper;
using Ecom.infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected readonly AppDbContext context;
    protected readonly IMapper mapper;

    public BaseController(AppDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }
}