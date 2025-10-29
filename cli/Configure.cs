using System.Data;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using arnold.Services;
using arnold.Utilities;

namespace arnold;

class Configure {
    public static CommandRouter EntryRouter = new (
        Name: "config",
        Description: "Manage configuration",
        Aliases: ["config", "configure"],
        Handler: static (string[] args) => {
        var configureRouting = new List<CommandRouter>([
            CommandProcessor.FallbackRouter, InfoRouter, RetargetRouter
        ]);

        var processor = new CommandProcessor(
            CommandRouters: configureRouting,
            HelpText: CommandProcessor.GenerateHelpText( [EntryRouter.Name], EntryRouter.Description, configureRouting ));
        return processor.Execute(args);
    } );

    public static CommandRouter InfoRouter = new (
        Name: "Info",
        Description: "Display configuration information",
        Aliases: [ "info" ],
        Handler: static (string[] args) => {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: [ new( "database", Description: "Get database path", ArgumentType.Flag ) ]
        );

        var emitAll = arguments.Keys.Count != 0;
        var emitDictionary = arguments.ContainsKey("database") || emitAll;

        using var dataService = Provider.GetRequiredService<ArnoldService>();
        if( emitDictionary ) Console.WriteLine($"Database Path: {dataService.DbPath}");
        return 0;
    } );

    public static CommandRouter RetargetRouter = new (
        Name: nameof(RetargetRouter).Replace("Router", ""),
        Description: "Retarget files in a library that were moved to a new location",
        Aliases: [ "retarget" ],
        Handler: static (string[] args) => {
            var arguments = ArgumentProcessor.Process( args, argumentMap: [
                new( "library", "Library", ArgumentType.Value, true, ["-l", "lib", "library"] ),
                new( "source", "Original root path", ArgumentType.Value, true, ["-s", "src", "source"] ),
                new( "target", "New root path", ArgumentType.Value, true, ["-t", "tgt", "target" ] ),
                new( "ignoreCase", "Case-Insensitive", ArgumentType.Flag, false, ["-C"] ),
                new( "dryRun", "List changes, don't apply", ArgumentType.Flag, false, ["-dr", "--dry"])
            ]);

            var libraryName = arguments["library"]!.First().ToLower();
            var source = arguments["source"]!.First().Replace('/', '\\');
            var target = arguments["target"]!.First().Replace('/', '\\');
            var dryRun = arguments.ContainsKey("dryRun");

            if( !source.EndsWith('\\') ) source += '\\';
            if( !target.EndsWith('\\') ) target += '\\';

            var ignoreCase = arguments.ContainsKey("ignoreCase");
            if( ignoreCase ) source = source.ToLower();

            var dataService = Provider.GetRequiredService<ArnoldService>();
            var library = dataService.Libraries.First( lib => lib.Name.ToLower() == libraryName );

            var stringComparer = ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;

            var sourceLength = source.Length;
            var sourceFiles = dataService.Metadata.Where( meta => meta.LibraryId == library.Id ).AsEnumerable();
            if( ignoreCase ) sourceFiles = sourceFiles.Where( meta => meta.Name.ToLower().StartsWith( source ) );
            else sourceFiles = sourceFiles.Where( meta => meta.Name.StartsWith( source ) );

            if( !dryRun ) Console.WriteLine($"Retargeting {source} to {target} - {sourceFiles.Count()} Entries");
            else Console.WriteLine($"Retargeting {source} to {target} - {sourceFiles.Count()} Entries [DRY RUN]");

            Console.CursorVisible = false;
            foreach( Models.FileMetadata meta in sourceFiles ) {
                meta.Name = target + meta.Name[sourceLength..];
            }
            Console.CursorVisible = true;
            Console.WriteLine();

            if( !dryRun ) dataService.SaveChanges();

            return 0;      
        }
    );
}