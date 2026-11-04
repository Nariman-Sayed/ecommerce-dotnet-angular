namespace Ecom.Application;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }

    DbSet<Product> Products { get; }

    DbSet<Photo> Photos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
