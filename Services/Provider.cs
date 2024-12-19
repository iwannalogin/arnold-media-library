using Microsoft.Extensions.DependencyInjection;

namespace arnold.Services;

class Provider {
    protected static ServiceCollection collection;
    protected static ServiceProvider? _instance;
    public static ServiceProvider Instance {
        get {
            if( _instance is null ) {
                _instance = collection.BuildServiceProvider();
            }
            return _instance;
        }
    }

    static Provider() {
        collection = new ServiceCollection();
        collection.AddDbContext<DataService>();
    }

    public static T? GetService<T>() where T : class
        => Instance.GetService<T>();

    public static T GetRequiredService<T>() where T : class
        => Instance.GetRequiredService<T>();
}