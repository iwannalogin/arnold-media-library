#!/bin/env -S dotnet run
#:property PublishAot=false
#:package Bogus@35.6.5

using System.Text.RegularExpressions;
using Bogus;

string[] variousExtensions = ["jpg", "gif", "png", "jpeg", "webp", "raw", "bmp", "wbmp", "mov", "mp4", "txt", "log", "cs", "tsx", "flag", "mp3"];
string[] imageExtensions = ["jpg", "gif", "png", "jpeg", "webp", "raw", "bmp", "wbmp"];

static string SanitizeName( string fileName ) {
    return fileName.Replace("\\", "-").Replace("/", "-").Replace(":", " - " );
}

Console.WriteLine("Starting Test Data Generation");

var seed = 423114;
Console.WriteLine($"Initializing faker with seed {seed}");
var faker = new Faker {
    Random = new Randomizer(seed)
};

var generateFile = Directory.EnumerateFiles( Environment.CurrentDirectory, "GenerateData.cs", SearchOption.AllDirectories ).FirstOrDefault();
if( generateFile is null ) {
    Console.WriteLine("Run this script in the same file GenerateData.cs is in.");
    return;
}

var dataDirectory = Path.Combine( Path.GetDirectoryName(generateFile)!, "data" );
Console.WriteLine($"Deleting \"{dataDirectory}\"");
Directory.Delete(dataDirectory, true);

Console.WriteLine($"Recreating \"{dataDirectory}\"");
Directory.CreateDirectory(dataDirectory);

var wallpaperDirectory = Path.Combine(dataDirectory, "wallpapers");
Console.WriteLine($"Creating \"wallpaper\" subdirectory");
Directory.CreateDirectory(wallpaperDirectory);

Console.WriteLine($"Generating \"wallpaper\" files");
for( var i = 0; i < faker.Random.Number(25, 50); i++ ) {
    File.WriteAllBytes(
        Path.Combine( wallpaperDirectory, SanitizeName( $"{faker.Random.Words(1)}-{faker.Random.Number(10, 2651)}.{faker.PickRandom(imageExtensions)}" ) ),
        new byte[faker.Random.Number(10, 600) * 1024]
    );
}

var photoDirectory = Path.Combine(dataDirectory, "photos");
Console.WriteLine($"Creating \"photos\" subdirectory");
Directory.CreateDirectory(photoDirectory);

Console.WriteLine($"Generating \"photos\" files");
string[] locations = [
  "",
  "New Folder",
  "New Folder(2)",
  ..Enumerable.Range(0, 5).Select( idx => faker.Address.City() )
];

var startDate = new DateTime(1950, 01, 01);
var endDate = new DateTime(2000, 12, 30);

foreach( var location in locations ) {
    var locationDirectory = string.IsNullOrWhiteSpace(location)
        ? photoDirectory
        : Path.Combine(photoDirectory, location);
    if( !Directory.Exists( locationDirectory ) ) Directory.CreateDirectory( locationDirectory );

    for( var i = 0; i < faker.Random.Number(25,50); i++ ) {
        File.WriteAllBytes(
            Path.Combine( locationDirectory, $"{faker.Date.Between(startDate, endDate):yyyy-MM-dd} - {i:00}.{faker.PickRandom(imageExtensions)}" ),
            new byte[faker.Random.Number(10, 600) * 1024] );
    }
}

var jumbleDirectory = Path.Combine(dataDirectory, "jumble");
Console.WriteLine("Creating \"jumble\" subdirectory");
Directory.CreateDirectory(jumbleDirectory);

Console.WriteLine("Generating \"jumble\" files");
foreach( var location in locations ) {
    var locationDirectory = string.IsNullOrWhiteSpace(location)
        ? jumbleDirectory
        : Path.Combine(jumbleDirectory, location);
    if( !Directory.Exists( locationDirectory ) ) Directory.CreateDirectory( locationDirectory );

    for( var i = 0; i < faker.Random.Number(25,50); i++ ) {
        File.WriteAllBytes(
            Path.Combine( locationDirectory, SanitizeName($"{faker.Random.Words(2)}.{faker.PickRandom(variousExtensions)}") ),
            new byte[faker.Random.Number(10, 600) * 1024] );
    }
}

Console.WriteLine("Done");