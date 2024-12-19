using arnold.Services;
using Microsoft.Extensions.DependencyInjection;

namespace arnold;

class Library {
    public static int Entry( string[] args ) {
        var libraryRouting = new List<CommandRouter>([
            new CommandRouter(
                Name: "fallback",
                Description: "FAILS",
                Handler: args => {
                    Console.WriteLine("Use the --help flag for available commands.");
                    return 0;
                },
                Aliases: Array.Empty<string>()
            ),
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
                Aliases: ["create", "new"]
            )
        ]);

        var processor = new CommandProcessor( libraryRouting, "HELP");
        return processor.Execute(args);
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
        var library = dataService.Libraries.Add( new Models.FileLibrary() {
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
                    Console.WriteLine( "\t" + descriptionLine );
                }
            }
        }
        return 0;
    }
}