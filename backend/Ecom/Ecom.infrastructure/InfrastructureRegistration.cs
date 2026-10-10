using Ecom.Application;
using Ecom.infrastructure.Data;
using Ecom.infrastructure.Repositries.Service;
using Ecom.infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Ecom.infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection InfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IImageManagementServices, ImageManagementService>();

        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        Directory.CreateDirectory(webRootPath);
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(webRootPath));

        services.AddDbContext<AppDbContext>(op =>
        {
            op.UseSqlServer(configuration.GetConnectionString("EcomDatabase"));
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        return services;
    }
}
