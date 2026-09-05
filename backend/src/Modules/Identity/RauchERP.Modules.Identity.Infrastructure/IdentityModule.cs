using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RauchERP.Modules.Identity.Application;

namespace RauchERP.Modules.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(o => o.UseNpgsql(configuration.GetConnectionString("Database")));
        services.AddScoped<IIdentityService, IdentityService>();
        return services;
    }
}
