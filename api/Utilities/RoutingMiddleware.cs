namespace arnold.Utilities;

public class RoutingMiddleware( RequestDelegate next, RoutingChain chain, IServiceProvider serviceProvider ) {
    public async Task InvokeAsync( HttpContext context ) {
        var routingLink = chain.RoutingLinks.FirstOrDefault( link => link.IsMatch(context.Request) );
        if( routingLink is not null ) {
            try {
                await routingLink.Invoke(context, serviceProvider );
            } catch( FileNotFoundException ) {
                context.Response.StatusCode = 404;
            } catch( ArgumentException ) {
                context.Response.StatusCode = 301;
            } catch {
                context.Response.StatusCode = 500;
            }
        } else {
            await next(context);
        }
    }
}

public static class RoutingMiddlewareExtensions {
    public static IServiceCollection AddArnoldChain( this IServiceCollection services, CommandDefinition rootCommand ) {
        services.AddSingleton( new RoutingChain(rootCommand) );
        return services;
    }

    public static IApplicationBuilder UseArnoldMiddleware( this IApplicationBuilder builder ) {
        builder.UseMiddleware<RoutingMiddleware>();
        return builder;
    }
}