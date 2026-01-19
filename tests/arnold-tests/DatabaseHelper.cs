using arnold;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace arnold_tests;

[TestClass]
public class DatabaseHelper {
    public static IConfiguration Configuration;
    public static string TestDatabase;

    static DatabaseHelper() {
        var configBuilder = new ConfigurationBuilder()
            .AddUserSecrets<ArnoldManagerTests>();
        Configuration = configBuilder.Build();

        TestDatabase = Configuration["TestDB"]!;    
    }

    private static Lock DataCopyLock = new Lock();
    public static ArnoldManager CreateManagerInstance() {
        var memoryConnection = new SqliteConnection("Data Source=:memory");
        memoryConnection.Open();
        lock( DataCopyLock ) {
            using var fileConnection = new SqliteConnection($"Data Source={TestDatabase}");
            fileConnection.Open();
            fileConnection.BackupDatabase( memoryConnection );
        }

        var dbContextBuilder = new DbContextOptionsBuilder();
        dbContextBuilder.UseSqlite( memoryConnection );
        var arnoldCTX = new ArnoldService( dbContextBuilder.Options );
        return new ArnoldManager( arnoldCTX );
    }

    [TestMethod]
    public void GetTestFile() {
        var testingDbPath = Configuration["TestDB"];
        Assert.IsFalse( String.IsNullOrWhiteSpace( testingDbPath ), "TestDB environment variable is set" );
        Assert.IsTrue( File.Exists(testingDbPath ), "testing.db database exists" );
    }
}