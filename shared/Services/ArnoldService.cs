using arnold.Models;
using Microsoft.EntityFrameworkCore;

namespace arnold;

public class ArnoldService : DbContext {
    public required DbSet<FileMetadata> Metadata { get; set; }
    public required DbSet<FileTag> Tags { get; set; }
    public required DbSet<FileLibrary> Libraries { get; set; }
    public required DbSet<FileAttributeDefinition> AttributeDefinitions { get; set; }
    public required DbSet<FileAttribute> Attributes { get; set; }
    public required DbSet<FileMonitor> Monitors { get; set; }

    public ArnoldService() : base() {}

    public ArnoldService( DbContextOptions options ): base(options) {}

    public static string GetDefaultDatabase() {
        var appData = Environment.SpecialFolder.LocalApplicationData;
        var appDataPath = Environment.GetFolderPath(appData, Environment.SpecialFolderOption.Create);
        return Path.Join( appDataPath, "arnold-media", "database.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        if( !options.IsConfigured ) {
            var dbPath = GetDefaultDatabase();
            var directory = Path.GetDirectoryName(dbPath);
            if( directory is not null && !Directory.Exists(directory) ) {
                Directory.CreateDirectory(directory);
            }
            options.UseSqlite($"Data Source={dbPath}");
        }
    }

    private static readonly Lock initializerLock = new();
    private static bool hasInitialized = false;
    public void Initialize() {
        if( hasInitialized ) return;

        lock(initializerLock) {
            if( !hasInitialized ) {
                Database.Migrate();
                hasInitialized = true;   
            }
        }
    }

    protected override void OnModelCreating( ModelBuilder builder ) {
        var thisAssembly = System.Reflection.Assembly.GetAssembly(typeof(ArnoldService))!;

        foreach( var type in thisAssembly.GetTypes() )
        {
            foreach( var method in type.GetMethods( System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public ) )
            {
                var builderAttribute = method.GetCustomAttributes( typeof( ModelBuilderAttribute ), true ).FirstOrDefault();
                if( builderAttribute != null )
                {
                    method.Invoke( null, [builder] );
                }
            }
        }

        base.OnModelCreating(builder);
    }
}