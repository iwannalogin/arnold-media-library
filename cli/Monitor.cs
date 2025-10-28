using arnold.Services;
using arnold.Utilities;
using Microsoft.EntityFrameworkCore;

namespace arnold;

class Monitor {
    public static int Entry( string[] args ) {
        return new CommandProcessor( [
            CommandProcessor.FallbackRouter,
            new CommandRouter(
                Name: "create",
                Description: "Create a new file system monitor",
                Handler: CreateMonitor,
                Aliases: ["create", "new", "add"]
            ),
            new CommandRouter(
                Name: "list",
                Description: "List file monitors attached to a library",
                Handler: ListMonitors,
                Aliases: ["ls", "list"]
            )
        ], "hELP").Execute(args);
    }

    protected static int ListMonitors( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Library",
                    Description: "Library to associate with monitor",
                    Type: ArgumentType.Value,
                    Aliases: ["-l", "--l", "-lib", "--lib", "-library", "--library" ]
                )
            ]
        );

        var libraryName = arguments.GetValueOrDefault("Library")?.FirstOrDefault();

        if( libraryName is null ) {
            Console.WriteLine("Not all required values provided");
            return -1;
        }

        var dataService = Provider.GetRequiredService<ArnoldService>();
        var library = dataService.Libraries
            .Where( lib => lib.Name.Equals(libraryName, StringComparison.CurrentCultureIgnoreCase))
            .Include( lib => lib.Monitors )
            .FirstOrDefault();

        if( library is null ) {
            Console.WriteLine( $"Failed to find library {libraryName}" );
            return -1;
        }

        if( !library.Monitors.Any() ) {
            Console.WriteLine( $"There are no monitors associated with library {libraryName}." );
        } else {
            foreach( var monitor in library.Monitors ) {
                Console.WriteLine( monitor.Name );
                Console.WriteLine( $"-- {monitor.Directory}{(monitor.Recurse ? " (RECURSE)" : "")}" );
                if( monitor.IsExclusionRule ) Console.WriteLine( $"-- Excluding: {monitor.Rule}" );
                else Console.WriteLine( $"-- Matching: {monitor.Rule}" );
            }
        }

        return 0;
    }

    protected static int CreateMonitor( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [
                new ArgumentDefinition(
                    Name: "Library",
                    Description: "Library to associate with monitor",
                    Type: ArgumentType.Value,
                    Aliases: ["-l", "--l", "-lib", "--lib", "-library", "--library" ]
                ),
                new ArgumentDefinition(
                    Name: "Name",
                    Description: "Name for the monitor",
                    Type: ArgumentType.Value,
                    Aliases: [ "-n", "--n", "-name", "--name" ]
                ),
                new ArgumentDefinition(
                    Name: "Rule",
                    Description: "Rule for the monitor as a regex",
                    Type: ArgumentType.Value,
                    Aliases: ["-rule", "--rule"]
                ),
                new ArgumentDefinition(
                    Name: "Recurse",
                    Description: "Apply recursively?",
                    Type: ArgumentType.Flag,
                    Aliases: ["-r", "--r", "-recurse", "--recurse", "-recursive", "--recursive" ]
                ),
                new ArgumentDefinition(
                    Name: "Directory",
                    Description: "Directory to monitor",
                    Type: ArgumentType.Value,
                    Aliases: [ "-d", "--d", "-dir", "--dir" ]
                ),
                new ArgumentDefinition(
                    Name: "Exclusion",
                    Description: "Is exclusion rule",
                    Type: ArgumentType.Flag,
                    Aliases: ["-e", "--e", "-ex", "--ex", "-exclude", "--exclude"]
                ),
                new ArgumentDefinition(
                    Name: "Interactive",
                    Description: "Run interactively",
                    Type: ArgumentType.Flag,
                    Aliases: ["-it", "--it", "-interactive", "--interactive"]
                ),
                new ArgumentDefinition(
                    Name: "Extensions",
                    Description: "File extensions to include",
                    Type: ArgumentType.List,
                    Aliases: ["-x", "--x", "-extension", "--extension", "-extensions", "--extensions"]
                )
            ]
        );

        var name = arguments.GetValueOrDefault("Name")?.FirstOrDefault();
        var libraryName = arguments.GetValueOrDefault("Library")?.FirstOrDefault();
        var rule = arguments.GetValueOrDefault("Rule")?.FirstOrDefault()
            ?? (!arguments.ContainsKey("Extensions") ? null : $".*\\.({string.Join("|", arguments["Extensions"]!.Select( ext => ext.TrimStart('.') ))})$");
            //^.*\.(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF)$
        var directory = arguments.GetValueOrDefault("Directory")?.FirstOrDefault()?.Replace("/", "\\");
        var recurse = arguments.ContainsKey("Recurse");
        var isExclusion = arguments.ContainsKey("Exclusion");
        var isInteractive = arguments.ContainsKey("Interactive");

        if( name is null
            || libraryName is null
            || rule is null
            || directory is null ) {
            
            Console.WriteLine("Not all required values provided");
            return 0;
        }

        if( Directory.Exists(directory) == false ) { 
            Console.WriteLine( $"Failed to find directory {directory}." );
            return -1;
        }

        var dataService = Provider.GetRequiredService<ArnoldService>();
        var library = dataService.Libraries.FirstOrDefault( lib => lib.Name.Equals(libraryName, StringComparison.CurrentCultureIgnoreCase));

        if( library is null ) {
            Console.WriteLine( $"Failed to find library named {libraryName},");
            return -1;
        }

        //foreach( var kvp in arguments ) {
        //    var key = kvp.Key;
        //    var value = kvp.Value;
        //    if( value is null ) Console.WriteLine( $"{key}: NULL" );
        //    else if( value.Count() == 0 ) Console.WriteLine( $"{key}: EMPTY" );
        //    else if( value.Count() == 1 ) Console.WriteLine( $"{key}: {value.FirstOrDefault()}" );
        //    else {
        //        Console.WriteLine( key );
        //        foreach( var val in value ) Console.WriteLine( $"-- {val}" );
        //    }
        //}
        //Console.WriteLine( "-d " + directory );

        dataService.Monitors.Add( new Models.FileMonitor() {
            Name = name,
            LibraryId = library.Id,
            Library = library,
            Directory = directory,
            Recurse = recurse,
            Rule = rule,
            IsInclusionRule = !isExclusion
        });
        dataService.SaveChanges();
        return 0;
    }
}