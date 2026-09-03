using AutoMapper;
using Ecom.Core.interfaces;
using Ecom.Core.Services;
using Ecom.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.infrastructure.Repositries
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageManagementServices _imageManagementServices;

        public ICategoryRepositry CategoryRepositry { get; }
        public IProductRepositry ProductRepositry { get; }
        public IPhotoRepositry PhotoRepositry { get; }

        public UnitOfWork(AppDbContext context,IMapper mapper,IImageManagementServices imageManagementServices)
        {
            _context = context;
            _mapper = mapper;
            _imageManagementServices = imageManagementServices;

            CategoryRepositry = new CategoryRepositry(_context);
            PhotoRepositry = new PhotoRepositry(_context);
            ProductRepositry = new ProductRepositry(_context, _mapper,      _imageManagementServices);
            _imageManagementServices = imageManagementServices;
        }
    }
}
