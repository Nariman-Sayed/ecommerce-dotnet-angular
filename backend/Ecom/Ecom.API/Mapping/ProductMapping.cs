using AutoMapper;
using Ecom.Core.DTO;
using Ecom.Core.Entities.Product;

namespace Ecom.API.Mapping
{
    public class ProductMapping : Profile
    {
        public ProductMapping()
        {
              CreateMap<Product, ProductDTO>().ForMember(x=>x.CategoryName,
                  op => op.MapFrom(src => src.Category.Name))
                .ReverseMap();
            CreateMap<Photo, PhotoDTO>().ReverseMap();
            CreateMap<AddProductDTO,Product>().ForMember(x => x.Photos, op => op.Ignore())
                .ReverseMap();

            CreateMap<updateProductDTO, Product>().ForMember(x => x.Photos, op => op.Ignore())
                .ReverseMap();
        }
    }
}
