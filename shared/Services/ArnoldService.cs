using arnold.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace arnold.Services;

public class ArnoldService : DbContext {
    public required DbSet<FileMetadata> Metadata { get; set; }
    public required DbSet<FileTag> Tags { get; set; }
    public required DbSet<FileLibrary> Libraries { get; set; }
    public required DbSet<FileAttributeDefinition> AttributeDefinitions { get; set; }
    public required DbSet<FileAttribute> Attributes { get; set; }
    public required DbSet<FileMonitor> Monitors { get; set; }

    public string DbPath { get; private set; }

    public ArnoldService() {
        var appData = Environment.SpecialFolder.LocalApplicationData;
        var appDataPath = Environment.GetFolderPath(appData, Environment.SpecialFolderOption.Create);
        DbPath = Path.Join( appDataPath, "arnold-media", "database.db" );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        var directory = Path.GetDirectoryName(DbPath);
        if( directory is not null && !Directory.Exists(directory) ) {
            Directory.CreateDirectory(directory);
        }
        options.UseSqlite($"Data Source={DbPath}");
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