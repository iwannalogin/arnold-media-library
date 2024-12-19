using System;

public readonly record struct CommandRouter( string Name, string Description, Func<string[], int> Handler, params string[] Aliases );

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
            if( router.Aliases.Any( alias => alias.ToLower() == command ) ) {
                return router.Handler( args.Skip(1).ToArray() );
            }
        }

        return defaultPath(args.Skip(1).ToArray() );
    }
}