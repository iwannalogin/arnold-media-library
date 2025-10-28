using System;
using System.Text;

namespace arnold.Utilities;

public readonly record struct CommandRouter( string Name, string Description, Func<string[], int> Handler, IEnumerable<string> Aliases, IEnumerable<CommandRouter>? SubCommands = null );

public class CommandProcessor( IEnumerable<CommandRouter> CommandRouters, string HelpText ) {
    /// <summary>
    /// Executes the command router given the supplied arguments.
    /// Falls back to the first entry if no arguments provided or
    /// commands match.
    /// </summary>
    /// <param name="args">Argument List</param>
    /// <returns>Return code</returns>
    public int Execute( string[] args ) {
        var defaultPath = CommandRouters.First().Handler;

        if( args.Length == 0 ) {
            return defaultPath( args );
        }

        string[] helpAliases = ["help", "-h", "--h", "-help", "--help", "-?", "--?"];

        var command = args[0].ToLower();
        if( helpAliases.Contains( command ) ) {
            Console.WriteLine(HelpText);
            return 0;
        }

        foreach( var router in CommandRouters ) {
            if( router.Aliases.Any( alias => alias.Equals(command, StringComparison.CurrentCultureIgnoreCase)) ) {
                return router.Handler([.. args.Skip(1)]);
            }
        }

        return defaultPath([.. args.Skip(1)]);
    }

    public static string AssemblyName = System.Reflection.Assembly.GetAssembly( typeof(CommandProcessor) )!.GetName()!.Name!;
    public static string GenerateHelpText( string[] command, string description, IEnumerable<CommandRouter>? routings = null, IEnumerable<ArgumentDefinition>? argumentMap = null ) {
        routings = routings is null ? [] : routings.Where( routing => routing.Name != "fallback" );
        argumentMap ??= [];

        var helpBuilder = new StringBuilder($"{AssemblyName} {string.Join(" ", command)}");
        helpBuilder.AppendLine(description);
        
        if( routings.Any() ) {
            helpBuilder.AppendLine("  Commands");
            helpBuilder.AppendLine("----------");
            foreach( var routing in routings ) {
                helpBuilder.AppendLine( $"  {routing.Name}: {routing.Description}" );
                if( routing.Aliases.Any() ) helpBuilder.AppendLine("    " + string.Join(" ", routing.Aliases) );
            }
        }

        if( argumentMap.Any() ) {
            helpBuilder.AppendLine("  Arguments");
            helpBuilder.AppendLine("-----------");
            foreach(var argument in argumentMap ) {
                helpBuilder.AppendLine($"  {argument.Name}: {argument.Description}" + (argument.Required ? " [required]" : "" ) );
                if( argument.Aliases is not null && argument.Aliases.Any() ) {
                    helpBuilder.AppendLine( "    " + string.Join(" ", argument.Aliases) );
                }
            }
        }
        return helpBuilder.ToString();
    }

    public static CommandRouter FallbackRouter = new(
        Name: "fallback",
        Description: "FAILS",
        Handler: args => {
            Console.WriteLine("Use the --help flag for available commands");
            return 0;
        },
        Aliases: []
    );
}