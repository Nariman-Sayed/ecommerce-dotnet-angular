namespace Ecom.infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var electronics = new Category("Electronics", "Devices and gadgets");
        var books = new Category("Books", "Reading material");

        context.Categories.AddRange(electronics, books);

        context.Products.AddRange(
            new Product("Laptop", "Portable computer", 0, 1199, 999) { Category = electronics },
            new Product("Novel", "Paperback fiction", 0, 25, 15) { Category = books });

        await context.SaveChangesAsync();
    }
}