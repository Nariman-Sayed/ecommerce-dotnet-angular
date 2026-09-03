using Ecom.infrastructure.Data;
using Ecom.infrastructure.Repositries.Service;
using Ecom.infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Ecom.infrastructure;

public static class infrastructureRegistration
{
    public static IServiceCollection infrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IImageManagementServices, ImageManagementService>();

        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(webRootPath);
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(webRootPath));

        services.AddDbContext<AppDbContext>(op =>
        {
            op.UseSqlServer(configuration.GetConnectionString("EcomDatabase"));
        });

        return services;
    }
}
