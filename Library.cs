using arnold.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace arnold;

class Library {
    public static int Entry( string[] args ) {
        var libraryRouting = new List<CommandRouter>([
            CommandProcessor.FallbackRouter,
            new CommandRouter(
                Name: "list",
                Description: "List existing libraries",
                Handler: ListLibraries,
                Aliases: ["list", "ls"]
            ),
            new CommandRouter(
                Name: "create",
                Description: "Create a new library",
                Handler: CreateLibrary,
                Aliases: ["create", "new", "add"]
            ),
            new CommandRouter(
                Name: "delete",
                Description: "Delete an existing library",
                Handler: DeleteLibrary,
                Aliases: ["delete", "remove"]
            ),
            new CommandRouter(
                Name: "update",
                Description: "Scan folders to update files in library",
                Handler: UpdateLibrary,
                Aliases: ["update", "scan"]
            )
        ]);

        var processor = new CommandProcessor( libraryRouting, "HELP");
        return processor.Execute(args);
    }

    public static int UpdateLibrary( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Library",
                    Description: "Name of library to update",
                    Type: ArgumentType.Value,
                    Aliases: ["-l", "--l", "-lib", "--lib", "-library", "--library" ]
                )
            ]
        );

        var libraryName = arguments.GetValueOrDefault( "Library" )?.FirstOrDefault();
        if( libraryName is null ) {
            Console.WriteLine("No library name provided.");
            return -1;
        }

        var dataService = Provider.GetRequiredService<DataService>();
        var library = dataService.Libraries.Include( lib => lib.Monitors ).FirstOrDefault( l => l.Name.ToLower() == libraryName.ToLower() );
        if( library is null ) {
            Console.WriteLine($"Failed to find library ${libraryName}");
            return -1;
        }

        //TODO: Parallelize
        var monitorsByDirectory = library.Monitors.GroupBy( mon => ( Directory: mon.Directory.ToLower(), Recurse: mon.Recurse ) );
        foreach( var monitorGroup in monitorsByDirectory ) {
            var directory = monitorGroup.Key.Directory;
            var recurse = monitorGroup.Key.Recurse;

            foreach( var file in Directory.EnumerateFiles( directory, "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly ) ) {
                var shouldAdd = monitorGroup.Any( monitor => monitor.IsMatch( file ) );

                if( shouldAdd && !dataService.Metadata.Any( info => info.LibraryId == library.Id && info.Name.ToLower() == file.ToLower() )) {
                    dataService.Metadata.Add( new Models.FileMetadata() {
                        Name = file,
                        Label = Path.GetFileName( file ),
                        LibraryId = library.Id
                    } );
                }
            }
            dataService.SaveChanges();
        }
        return 0;
    }

    public static int DeleteLibrary( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Name",
                    Description: "Library name",
                    Type: ArgumentType.Value,
                    Aliases: [ "-n", "--n", "-name", "--name" ]
                )
            ]
        );

        if( !arguments.ContainsKey("Name") ) return 0;
        var name = arguments["Name"]!.First();

        var dataService = Provider.GetRequiredService<DataService>();
        var library = dataService.Libraries.FirstOrDefault( l => l.Name.ToLower() == name.ToLower() );
        if( library is null ) {
            Console.WriteLine($"Failed to find library ${name}");
            return -1;
        }

        dataService.Libraries.Remove( library );
        dataService.SaveChanges();

        Console.WriteLine($"Deleted library {name}");
        return 0;
    }

    public static int CreateLibrary( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Name",
                    Description: "Library name",
                    Type: ArgumentType.Value,
                    Aliases: ["-n", "--n", "-name", "--name"]
                ),
                new ArgumentDefinition(
                    Name: "Description",
                    Description: "Library description",
                    Type: ArgumentType.Value,
                    Aliases: ["-d", "--d", "-desc", "--desc", "-description", "--description"]
                ),
                new ArgumentDefinition(
                    Name: "Interactive",
                    Description: "Interactively create library",
                    Type: ArgumentType.Flag,
                    Aliases: ["-it", "--it"]
                )
            ]
        );

        if( arguments.ContainsKey("Interactive") ) {
            if( !arguments.ContainsKey("Name") ) {
                Console.WriteLine("Library Name:" );
                var buffer = Console.ReadLine();
                if( string.IsNullOrWhiteSpace(buffer) ) {
                    Console.WriteLine("No name provided.");
                    return 0;
                } else arguments.Add("Name", [buffer]);
            }

            if( !arguments.ContainsKey("Description") ) {
                Console.WriteLine("Library Description:");
                var buffer = Console.ReadLine() ?? string.Empty;
                arguments.Add("Description", [buffer]);
            }
        }

        var libraryName = arguments.GetValueOrDefault("Name")?.FirstOrDefault();
        var libraryDesc = arguments.GetValueOrDefault("Description")?.FirstOrDefault();

        if( libraryName is null || libraryDesc is null ) {
            Console.WriteLine("Not all required values provided.");
            return 0;
        }

        var dataService = Provider.GetRequiredService<DataService>();
        dataService.Libraries.Add( new Models.FileLibrary() {
            Name = libraryName,
            Description = libraryDesc
        } );
        dataService.SaveChanges();
        return 0;
    }

    public static int ListLibraries( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args, 
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Descriptions",
                    Description: "Include descriptions",
                    Type: ArgumentType.Flag,
                    Aliases: ["-d", "--d", "-desc", "--desc", "-description", "--description", "-descriptions", "--descriptions"]
                ),
                new ArgumentDefinition(
                    Name: "Count",
                    Description: "Include file count",
                    Type: ArgumentType.Flag,
                    Aliases: ["-c", "--c", "-count", "--count"]
                ),
                new ArgumentDefinition(
                    Name: "Full",
                    Description: "Include all details",
                    Type: ArgumentType.Flag,
                    Aliases: ["-f", "--f", "-full", "--full"]
                ),
                new ArgumentDefinition(
                    Name: "JSON",
                    Description: "Output as JSON",
                    Type: ArgumentType.Flag,
                    Aliases: ["-json", "--json"]
                )
        ]);

        var dataService = Provider.GetRequiredService<DataService>();

        if( dataService.Libraries.Any() == false ) {
            Console.WriteLine( "No libraries have been created.");
            return 0;
        }

        foreach( var library in dataService.Libraries ) {
            Console.WriteLine( library.Name );
            if( arguments.ContainsKey("Descriptions") || arguments.ContainsKey("Full") ) {
                foreach( var descriptionLine in library.Description.Split("\r\n") ) {
                    //TODO: Properly consider buffwer weidth
                    Console.WriteLine( ">> " + descriptionLine );
                }
            }

            if( arguments.ContainsKey("Count") || arguments.ContainsKey("Full") ) {
                Console.WriteLine(">> " + dataService.Metadata.Where( info => info.LibraryId == library.Id ).Count() );
            }
        }
        return 0;
    }
}