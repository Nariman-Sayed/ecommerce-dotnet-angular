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
        await context.SaveChangesAsync();

        context.Products.AddRange(
            new Product("Laptop", "Portable computer", electronics.Id, 1199, 999),
            new Product("Phone", "Smartphone device", electronics.Id, 899, 749),
            new Product("Tablet", "Touch screen tablet", electronics.Id, 499, 429),
            new Product("Headphones", "Wireless audio", electronics.Id, 199, 149),
            new Product("Camera", "Digital camera", electronics.Id, 699, 599),
            new Product("Novel", "Paperback fiction", books.Id, 25, 15),
            new Product("Cookbook", "Recipes and tips", books.Id, 30, 22),
            new Product("Biography", "Life story", books.Id, 20, 12),
            new Product("Atlas", "World maps", books.Id, 35, 28),
            new Product("Poetry", "Collected poems", books.Id, 18, 10));

        await context.SaveChangesAsync();
    }
}