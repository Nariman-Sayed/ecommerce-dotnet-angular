using AutoMapper;
using Ecom.API.Helper;
using Ecom.Core.DTO;
using Ecom.Core.interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : BaseController
    {
        public ProductsController(IUnitOfWork work, IMapper mapper) : base(work, mapper)
        {
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> get()
        {
            try
            {
                var Product = await work.ProductRepositry.GetAllAsync(x => x.Category, x => x.Photos);
                var result = mapper.Map<List<ProductDTO>>(Product);
                if (Product is null)
                    return BadRequest(new ResponseAPI(400));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Add-Product")]
        public async Task<IActionResult> add(AddProductDTO productDTO)
        {
            try {
                await work.ProductRepositry.AddAsync(productDTO);
                return Ok(new ResponseAPI(200));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseAPI(400, ex.Message));
            }
        }
        [HttpPut("Update-Product")]
        public async Task<IActionResult> Update(updateProductDTO updateProductDTO)
        {
            try
            {
                await work.ProductRepositry.UpdateAsync(updateProductDTO);
                return Ok(new ResponseAPI(200));
            }
            catch (Exception ex)
            {

                return BadRequest(new ResponseAPI(400, ex.Message));
            }
        }
        [HttpDelete("Delete-Product/{Id}")]
        public async Task<IActionResult> delete(int Id)
        {
            try
            {
                var product = await work.ProductRepositry
                    .GetByIdAsync(Id, x => x.Photos, x => x.Category);

                await work.ProductRepositry.DeleteAsync(product);

                return Ok(new ResponseAPI(200));

            }
            catch (Exception ex)
            {

                return BadRequest(new ResponseAPI(400, ex.Message));
            }
        }

    }
}
