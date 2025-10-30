using System.Collections;
using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace arnold.Utilities;

public class CommandFactory( IServiceProvider serviceProvider ) {
    public Command CreateCommand( CommandDefinition definition ) {
        var arguments = SortArguments( definition.ArgumentDefinitions ?? [] );

        var cmd = new Command( definition.Name.ToLower(), definition.Description );
        AttachHandler( cmd, definition );
        
        var firstRequiredList = true;
        foreach( var arg in arguments ) {
            if( arg.Required && arg.Type == ArgumentType.Value ) {
                cmd.Arguments.Add( new Argument<string>( arg.Name ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.ExactlyOne,
                } );
            } else if( arg.Required && arg.Type == ArgumentType.List && firstRequiredList ) {
                firstRequiredList = false;
                cmd.Arguments.Add( new Argument<string[]>( arg.Name ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.OneOrMore
                } );
            } else if( arg.Type == ArgumentType.Value ) {
                cmd.Options.Add( new Option<string>( arg.Name, [.. arg.Aliases] ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.ZeroOrOne
                } );
            } else if( arg.Type == ArgumentType.List ) {
                cmd.Options.Add( new Option<string[]>( arg.Name, [.. arg.Aliases] ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.ZeroOrMore
                } );
            } else {
                cmd.Options.Add( new Option<bool>( arg.Name, [.. arg.Aliases] ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.ZeroOrOne
                } );
            }
        }

        foreach( var subrouting in definition.SubCommands ?? [] ) {
            cmd.Subcommands.Add( CreateCommand( subrouting ) );
        }

        return cmd;
    }
    
    public static (bool IsMissing, object? Value) ProcessArgument( ParameterInfo parameter, ArgumentDefinition definition, object? suppliedValue ) {
        if( suppliedValue is not null ) {
            return (false, suppliedValue);
        } else if( definition.Required ) {
            return (true, suppliedValue);
        } else if( parameter.HasDefaultValue ) {
            return (false, parameter.DefaultValue);
        } else {
            return (false, null);
        }
    }

    private static IEnumerable<ArgumentDefinition> SortArguments( IEnumerable<ArgumentDefinition> arguments )
        => arguments
            .Select( (arg,idx) => (def: arg,idx) )
            .OrderBy( arg => arg.def.Required )
            .ThenBy( arg => arg.def.Type switch {
                ArgumentType.Value => 0,
                ArgumentType.List => 1,
                _ => 2
            })
            .ThenBy( arg => arg.idx )
            .Select( arg => arg.def );

    
    public void AttachHandler( Command command, CommandDefinition definition ) {
        if( definition.Handler is null ) return;

        command.SetAction( parsedArgs => {
            var paramList = new List<object?>();
            var missingParams = new List<string>();
            var serviceList = new List<object>();

            try {
                foreach( var paramInfo in definition.Handler.Method.GetParameters() ) {
                    if( paramInfo.GetCustomAttribute<FromServicesAttribute>() is not null ) {
                        var service = serviceProvider.GetRequiredService( paramInfo.ParameterType );
                        paramList.Add( service );
                        serviceList.Add( service );
                    } else {
                        var (isMissing, argValue) = ProcessArgument(
                            parameter: paramInfo,
                            definition: definition.ArgumentDefinitions.First( arg => arg.Name == paramInfo.Name ),
                            suppliedValue: parsedArgs.GetValue<object?>( paramInfo.Name! ) );
                        
                        paramList.Add(argValue);
                        if( isMissing ) missingParams.Add( paramInfo.Name! );
                    }
                }
                if( missingParams.Count != 0) throw new MissingArgumentsException( missingParams );

                var results = definition.Handler.Method.Invoke( definition.Handler.Target, [.. paramList ] );
                if( results is not null ) OutputResults(results);
            } catch {
                throw;
            } finally {
                foreach( var service in serviceList ) {
                    if( service is IDisposable disposableService ) {
                        disposableService.Dispose();
                    }
                }
                serviceList.Clear();
            }
        } );
    }

    public void OutputResults( object results ) {
        if( results.GetType().IsArray || results.GetType().IsAssignableTo( typeof( IEnumerable ) ) ) {
            foreach( var item in results as IEnumerable<object> ?? [] ) {
                Console.WriteLine(item);
            }
        } else Console.WriteLine(results);
    }
}