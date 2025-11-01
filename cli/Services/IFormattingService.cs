using System.Reflection;
using arnold.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace arnold.Services;

public interface IFormattingService {
    public void Configure( IEnumerable<string> options );
    public string Print( object item );
}

public static class IFormattingServiceExtensions {
    public static IServiceCollection AddFormattingServices( this IServiceCollection serviceCollection ) {
        foreach( var type in Assembly.GetExecutingAssembly().GetTypes() ) {
            var fsAttr = type.GetCustomAttribute<FormattingServiceAttribute>();
            if( fsAttr is null ) continue;

            serviceCollection.AddKeyedTransient( typeof(IFormattingService), fsAttr.Key, type );
        }

        return serviceCollection;
    }
}