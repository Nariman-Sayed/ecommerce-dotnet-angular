using AutoMapper;
using Ecom.Core.DTO;
using Ecom.Core.Entities.Product;
using Ecom.Core.interfaces;
using Ecom.Core.Services;
using Ecom.infrastructure.Data;
using Ecom.infrastructure.Repositries.Service;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.infrastructure.Repositries
{
    public class ProductRepositry : GenericRepositry<Product>, IProductRepositry
    {
        private readonly IMapper mapper;
        private readonly AppDbContext context;
        private readonly IImageManagementServices imageManagementServices;
        public ProductRepositry(AppDbContext context, IMapper mapper, IImageManagementServices imageManagementServices) : base(context)
        {
            this.mapper = mapper;
            this.context = context;
            this.imageManagementServices = imageManagementServices;
        }

        public async Task<bool> AddAsync(AddProductDTO productDTO)
        {
            if(productDTO == null)
            {
                return false;
            }
            var product = mapper.Map<Product>(productDTO);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            var ImagePath =await imageManagementServices.AddImageAsync(productDTO.Photo,productDTO.Name);

            var photo = ImagePath.Select(Path =>new Photo{
                ImageName = Path,
                ProductId= product.Id,
            }).ToList();
            await context.Photos.AddRangeAsync(photo);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAsync(Product product)
        {
            var photo = await context.Photos.Where(m => m.ProductId == product.Id)
                .ToListAsync();
            foreach(var item in photo)
            {
                imageManagementServices.DeleteImageAsync(item.ImageName);
            }
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAsync(updateProductDTO updateProductDTO)
        {
            if (updateProductDTO is null)
            {
                return false;
            }
            var FindProduct = await context.Products.Include(m => m.Category)
                .Include(m => m.Photos)
                .FirstOrDefaultAsync(m => m.Id == updateProductDTO.Id);

            if (FindProduct is null)
            {
                return false;
            }
            mapper.Map(updateProductDTO, FindProduct);

            var FindPhoto = await context.Photos.Where(m => m.ProductId == updateProductDTO.Id).ToListAsync();

            foreach (var item in FindPhoto)
            {
                imageManagementServices.DeleteImageAsync(item.ImageName);
            }
            context.Photos.RemoveRange(FindPhoto);

            var ImagePath = await imageManagementServices.AddImageAsync(updateProductDTO.Photo, updateProductDTO.Name);

            var photo = ImagePath.Select(path => new Photo
            {
                ImageName = path,
                ProductId = updateProductDTO.Id,
            }).ToList();

            await context.Photos.AddRangeAsync(photo);

            await context.SaveChangesAsync();
            return true;

        }
    }
}
