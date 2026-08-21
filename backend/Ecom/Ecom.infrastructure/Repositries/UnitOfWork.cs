using Ecom.Core.interfaces;
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

        public ICategoryRepositry CategoryRepositry { get; }
        public IProductRepositry ProductRepositry { get; }
        public IPhotoRepositry PhotoRepositry { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            CategoryRepositry = new CategoryRepositry(_context);
            PhotoRepositry = new PhotoRepositry(_context);
            ProductRepositry = new ProductRepositry(_context);

        }
    }
}
