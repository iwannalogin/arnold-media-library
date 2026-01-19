using arnold;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace arnold_tests;

[TestClass]
public sealed class ArnoldManagerTests {
    [TestMethod]
    public void GetLibraryTest() {
        var arnold = DatabaseHelper.CreateManagerInstance();

        Assert.IsNotNull( arnold.GetLibrary("wallpapers") );
        Assert.IsNotNull( arnold.GetLibrary("WALLPAPERS") );
        Assert.IsNotNull( arnold.GetLibrary("walLpaPers") );
        Assert.IsNull( arnold.GetLibrary( "walpapers" ) );
    }

    [TestMethod]
    public void AddLibraryTest() {
        var arnold = DatabaseHelper.CreateManagerInstance();

        var libraryName = "fake";
        var libraryDesc = "A fake library for testing";

        arnold.AddLibrary(libraryName, libraryDesc);
        var libraryInst = arnold.GetLibrary(libraryName);

        Assert.IsNotNull( libraryInst );
        Assert.AreEqual(libraryName, libraryInst.Name);
        Assert.AreEqual(libraryDesc, libraryInst.Description);
    }

    [TestMethod]
    public void DeleteLibraryTest() {
        var arnold = DatabaseHelper.CreateManagerInstance();
        arnold.DeleteLibrary( arnold.GetLibrary("wallpapers")! );
        Assert.IsNull( arnold.GetLibrary("wallpapers") );
    }

}