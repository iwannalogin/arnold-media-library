using arnold.Services;
using arnold.Utilities;
using Microsoft.EntityFrameworkCore;

namespace arnold;

class Tag {
    public static int Entry( string[] args ) {
        var tagRouting = new List<CommandRouter>([
            CommandProcessor.FallbackRouter,
            new CommandRouter(
                Name: "Directory",
                Description: "Tag all files in a directory",
                Handler: TagDirectory,
                Aliases: [ "dir", "directory" ]
            ),
            new CommandRouter(
                Name: "Files",
                Description: "Tag all files entered",
                Handler: TagFile,
                Aliases: [ "file", "files" ]
            )
        ]);

        var processor = new CommandProcessor(tagRouting, "HELP TEXT");
        return processor.Execute(args);
    }

    public static int TagFile( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Files",
                    Description: "Files to tag",
                    Type: ArgumentType.List,
                    Aliases: ["-f", "--f", "-file", "--file", "-files", "--files"]
                ),
                new ArgumentDefinition(
                    Name: "Library",
                    Description: "Library to tag",
                    Type: ArgumentType.Value,
                    Aliases: ["-l", "--l", "-lib", "--lib", "-library", "--library"]
                ),
                new ArgumentDefinition(
                    Name: "Tags",
                    Description: "Tags to apply",
                    Type: ArgumentType.List,
                    Aliases: ["-t", "--t", "-tag", "--tag", "-tags", "--tags"]
                ),
                new ArgumentDefinition(
                    Name: "Interactive",
                    Description: "Interactively create library",
                    Type: ArgumentType.Flag,
                    Aliases: ["-it", "--it"]
                )
            ]
        );

        var files = arguments["Files"]!;
        var libraryName = arguments.GetValueOrDefault("Library")?.FirstOrDefault();
        var isInteractive = arguments.ContainsKey("Interactive");
        var tagList = arguments["Tags"]!;

        //foreach( var kvp in arguments ) {
        //    Console.WriteLine( $"{kvp.Key}: {kvp.Value}" );
        //}

        if( files.Count() == 0 && tagList.Count() > 0 && isInteractive ) {
            Console.WriteLine("Add files:");
            var readLine = (out string? fileName) => {
                fileName = Console.ReadLine();
                return String.IsNullOrWhiteSpace(fileName) == false;
            };
            files = [];
            while( readLine(out var file) ) {
                (files as List<string>)!.Add(file!);
            }
        } else if ( tagList.Count() == 0 && files.Count() > 0 && isInteractive ) {
            Console.WriteLine("Add tags:");
            var readLine = (out string? tag) => {
                tag = Console.ReadLine();
                return String.IsNullOrWhiteSpace(tag) == false;
            };
            tagList = [];
            while( readLine(out var tag) ) {
                (tagList as List<string>)!.Add(tag!);
            }
        }

        //Do checks and interactive pass here.

        var dataService = Provider.GetRequiredService<ArnoldService>();
        var library = dataService.Libraries.First( lib => lib.Name.Equals(libraryName, StringComparison.CurrentCultureIgnoreCase));
        foreach( var fileName in files.Select( f => f.Trim('\"') ) ) {
            var metadata = dataService.Metadata.Include( info => info.Tags ).FirstOrDefault( info => info.LibraryId == library.Id && info.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase));
            if( metadata is null ) continue;

            var toAdd = tagList
                .Where( tag => !metadata.ContainsTag(tag) )
                .Select( tag => new Models.FileTag() {
                    FileId = metadata.Id,
                    Tag = tag
                } );

            dataService.Tags.AddRange( toAdd );
        }
        dataService.SaveChanges();
        //dataService.Dispose();

        return 0;
    }

    public static int TagDirectory( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Directory",
                    Description: "Directory of files to tag",
                    Type: ArgumentType.Value,
                    Required: true,
                    Aliases: ["-d", "--d", "-dir", "--dir", "-directory", "--directory"]
                ),
                new ArgumentDefinition(
                    Name: "Library",
                    Description: "Library to tag",
                    Type: ArgumentType.Value,
                    Required: true,
                    Aliases: ["-l", "--l", "-lib", "--lib", "-library", "--library"]
                ),
                new ArgumentDefinition(
                    Name: "Tags",
                    Description: "Tags to apply",
                    Type: ArgumentType.List,
                    Required: true,
                    Aliases: ["-t", "--t", "-tag", "--tag", "-tags", "--tags"]
                ),
                new ArgumentDefinition(
                    Name: "Interactive",
                    Description: "Interactively create library",
                    Type: ArgumentType.Flag,
                    Aliases: ["-it", "--it"]
                ),
                new ArgumentDefinition(
                    Name: "Recurse",
                    Description: "Apply recursively?",
                    Type: ArgumentType.Flag,
                    Aliases: ["-r", "--r", "-recurse", "--recurse", "-recursive", "--recursive" ]
                )
            ]
        );

        var directory = arguments["Directory"]!.First();
        var libraryName = arguments["Library"]!.First();
        var isInteractive = arguments.ContainsKey("Interactive");
        var recurse = arguments.ContainsKey("Recurse");
        var tagList = arguments["Tags"]!;

        //Do checks and interactive pass here.

        var dataService = Provider.GetRequiredService<ArnoldService>();
        var library = dataService.Libraries.First( lib => lib.Name.Equals(libraryName, StringComparison.CurrentCultureIgnoreCase));
        foreach( var fileName in Directory.EnumerateFiles( directory, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) ) {
            var metadata = dataService.Metadata.Include( info => info.Tags ).FirstOrDefault( info => info.LibraryId == library.Id && info.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase));
            if( metadata is null ) continue;

            var toAdd = tagList
                .Where( tag => !metadata.ContainsTag(tag) )
                .Select( tag => new Models.FileTag() {
                    FileId = metadata.Id,
                    Tag = tag
                } );

            dataService.Tags.AddRange( toAdd );
        }
        dataService.SaveChanges();
        //dataService.Dispose();

        return 0;
    }
}