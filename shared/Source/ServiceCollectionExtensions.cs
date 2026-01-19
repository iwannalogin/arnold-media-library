using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace arnold;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddArnold( this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsBuilder = null ) {
        services.AddDbContext<ArnoldService>( optionsBuilder );
        services.AddSingleton<ArnoldManager>();
        return services;
    }
}