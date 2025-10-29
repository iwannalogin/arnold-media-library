using System;
using System.Text;

namespace arnold.Utilities;

public readonly record struct CommandRouter(
    string Name,
    string Description,
    Delegate Handler,
    IEnumerable<string> Aliases,
    IEnumerable<ArgumentDefinition>? Arguments = null,
    IEnumerable<CommandRouter>? SubCommands = null ) {
    
    public IEnumerable<string> TestAliases {
        get => [ ToKebabCase(Name), .. Aliases ?? [] ];
    }

    public static string ToKebabCase( string value )
        => value.ToLower().Replace(" ", "-");
};

public class CommandProcessor( IEnumerable<CommandRouter> CommandRouters, string HelpText ) {
    /// <summary>
    /// Executes the command router given the supplied arguments.
    /// Falls back to the first entry if no arguments provided or
    /// commands match.
    /// </summary>
    /// <param name="args">Argument List</param>
    /// <returns>Return code</returns>
    public int Execute( string[] args ) {
        var router = CommandRouters.First();

        if( args.Length == 0 ) {
            Invoke( [], [], router );
            return 0;
        }

        var routers = CommandRouters;
        var activeArgs = args;

        while( routers.Any() ) {
            var nextRouter = routers
                .FirstOrDefault( router => router.TestAliases
                    .Any( alias => alias
                        .Equals( activeArgs[0], StringComparison.OrdinalIgnoreCase ) ) );
            if( !string.IsNullOrWhiteSpace( nextRouter.Name ) ) {
                router = nextRouter;
                routers = router.SubCommands ?? [];
                activeArgs = [..activeArgs.Skip(1)];
            }
        }

        Invoke(
            commands: [.. args.Take(activeArgs.Length - args.Length)],
            args: activeArgs,
            router: router );
        return 0;
    }

    static string[] helpAliases = ["help", "-h", "--h", "-help", "--help", "-?", "--?"];
    private static bool HasHelpFlag( string[] args )
        => args.Any( arg => 
            helpAliases.Any( alias => alias.Equals( arg, StringComparison.OrdinalIgnoreCase ) ));

    public void Invoke( string[] commands, string[] args, CommandRouter router ) {
        if( HasHelpFlag(args) ) {
            Console.WriteLine( GenerateHelpText(
                command: commands,
                description: router.Description,
                routings: router.SubCommands,
                argumentMap: router.Arguments
            ) );
            return;
        }

        var parameters = router.Handler.Method.GetParameters();

        if( parameters.Length == 0 ) {
          router.Handler.DynamicInvoke();  
        } else if( parameters.Length == 1 && parameters[0].ParameterType == typeof(string[]) ) {
            router.Handler.DynamicInvoke([args]);
            return;
        }
        var arguments = ArgumentProcessor.Process( args, router.Arguments ?? [] );
        var parameterList = new List<object?>();

        foreach( var param in parameters ) {
            object? paramValue = null;
            if( arguments.TryGetValue( param.Name!, out var valueList ) ) {
                var argDef = router.Arguments!.First( arg => arg.Name == param.Name );
                if( argDef.Type == ArgumentType.Value ) {
                    paramValue = ParseArgument( valueList?.FirstOrDefault(), param.ParameterType );
                } else if( argDef.Type == ArgumentType.Flag ) {
                    paramValue = true;
                } else {
                    paramValue = ParseArguments( valueList, param.ParameterType.GetElementType()! );
                }
            } else if( param.HasDefaultValue ) {
                paramValue = param.DefaultValue;
            }

            parameterList.Add( paramValue );
        }
        router.Handler.Method.Invoke(router.Handler.Target, [..parameterList] );
    }

    private static object? ParseArguments( IEnumerable<string?>? args, Type targetType ) {
        if( args is null ) return null;
        var parsedArgs = args.Select( arg => ParseArgument(arg, targetType ) ).ToArray();
        var arr = Array.CreateInstance( targetType, parsedArgs.Length );
        parsedArgs.CopyTo( arr, 0 );
        return arr;
    }

    private static object? ParseArgument( string? arg, Type targetType ) {
        if( arg is null ) return null;
        return Type.GetTypeCode(targetType) switch {
            TypeCode.String  => arg,
            TypeCode.Boolean => !string.IsNullOrWhiteSpace(arg),
            TypeCode.Int16 => short.Parse( arg ),
            TypeCode.Int32 => int.Parse( arg ),
            TypeCode.Int64 => long.Parse( arg ),
            TypeCode.UInt16 => ushort.Parse( arg ),
            TypeCode.UInt32 => uint.Parse( arg ),
            TypeCode.UInt64 => ulong.Parse( arg ),
            TypeCode.Single => float.Parse( arg ),
            TypeCode.Double => double.Parse( arg ),
            TypeCode.Char => arg[0],
            TypeCode.Byte => byte.Parse( arg ),
            TypeCode.DateTime => DateTime.Parse( arg ),
            _ => throw new NotImplementedException()
        };
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
        Handler: ( string[] args ) => {
            Console.WriteLine("Use the --help flag for available commands");
        },
        Aliases: []
    );
}