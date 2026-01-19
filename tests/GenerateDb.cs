#!/bin/env -S dotnet run
#:property PublishAot=false
#:project ../shared/arnold-shared.csproj

using arnold;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var generateFile = Directory.EnumerateFiles( Environment.CurrentDirectory, "GenerateDb.cs", SearchOption.AllDirectories ).FirstOrDefault();
if( generateFile is null ) {
    Console.WriteLine("Run this script in the folder GenerateDb.cs is in.");
    return;
}
var testDirectory = Path.GetDirectoryName(generateFile)!;
var databaseFile = Path.Combine( testDirectory, "data", "testing.db" );

Console.WriteLine("Starting test database generation");

if( File.Exists(databaseFile ) ) {
    Console.WriteLine("Deleting existing test database");
    File.Delete(databaseFile);
}

var serviceCollection = new ServiceCollection();
serviceCollection.AddArnold( options => {
   options.UseSqlite($"Data Source={databaseFile}");
});

var provider = serviceCollection.BuildServiceProvider();
var arnold = provider.GetRequiredService<ArnoldManager>();

Console.WriteLine("Creating \"wallpapers\" library.");
var wallpapers = arnold.AddLibrary( "wallpapers", "My favorite wallpapers!");
Console.WriteLine("Adding monitors to \"wallpapers.\"");
arnold.AddMonitor(
    library: wallpapers,
    monitorName: "images",
    directory: Path.Combine(testDirectory, "data", "wallpapers" ),
    rule: ".*\\.(png|jpg|jpeg|bmp|webp|tif|tiff|gif)$",
    recurse: true
);
Console.WriteLine("Updating \"wallpapers.\"");
arnold.RunMonitors(wallpapers);

Console.WriteLine("Creating \"photos\" library.");
var photos = arnold.AddLibrary( "photos", "Family Photos" );

Console.WriteLine("Adding monitors to \"photos.\"");
arnold.AddMonitor(
    library: photos,
    monitorName: "Trip Photos",
    directory: Path.Combine(testDirectory, "data", "photos"),
    rule: ".*\\.(png|jpg|jpeg|bmp|webp|tif|tiff|gif)$",
    recurse: true
);

arnold.AddMonitor(
    library: photos,
    monitorName: "Need to sort",
    directory: Path.Combine(testDirectory, "data", "photos"),
    rule: ".*[\\\\\\/]New Folder.*?[\\\\\\/].*",
    recurse: true,
    isInclusion: false
);

arnold.AddMonitor(
    library: photos,
    monitorName: "Needs to sourt2",
    directory: Path.Combine(testDirectory, "data", "photos"),
    isInclusion: false
);
Console.WriteLine("Updating \"photos.\"");
arnold.RunMonitors(photos);

Console.WriteLine("Creating \"jumble\" library.");
var jumble = arnold.AddLibrary("jumble", "Assorted Files" );

Console.WriteLine("Adding monitors to \"jumble.\"");
arnold.AddMonitor(
    library: jumble,
    monitorName: "Notes",
    directory: Path.Combine(testDirectory, "data", "jumble"),
    rule: ".*\\.(txt|log)",
    recurse: true
);
Console.WriteLine("Updating \"jumble.\"");
arnold.RunMonitors(jumble);