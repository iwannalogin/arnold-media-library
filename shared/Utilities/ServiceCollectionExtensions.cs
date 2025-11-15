using arnold.Managers;
using arnold.Services;
using Microsoft.Extensions.DependencyInjection;

namespace arnold.Utilities;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddArnold( this IServiceCollection services ) {
        services.AddDbContext<ArnoldService>();
        services.AddSingleton<LibraryManager>();
        services.AddSingleton<MetaManager>();
        return services;
    }
}