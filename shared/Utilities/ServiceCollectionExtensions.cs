using arnold.Services;
using Microsoft.Extensions.DependencyInjection;

namespace arnold.Utilities;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddArnold( this IServiceCollection services ) {
        services.AddDbContext<ArnoldService>();
        services.AddSingleton<ArnoldManager>();
        return services;
    }
}