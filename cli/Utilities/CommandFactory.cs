using System.Collections;
using System.CommandLine;
using System.CommandLine.Help;
using System.Reflection;
using System.Text;
using arnold.Services;
using Microsoft.Extensions.DependencyInjection;

namespace arnold.Utilities;

public class CommandFactory( IServiceProvider serviceProvider ) {
    public Command CreateCommand( CommandDefinition definition, bool isRoot = true ) {
        var arguments = SortArguments( definition.ArgumentDefinitions ?? [] );

        var cmd = isRoot
            ? new RootCommand( definition.Description )
            : new Command( definition.Name.ToLower(), definition.Description );
        if( !isRoot ) {
            var helpOption = new HelpOption();
            cmd.Options.Add( helpOption );
        }

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
                cmd.Options.Add( new Option<string>( GetOptionName(arg), [.. GetOptionAliases(arg)] ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.ZeroOrOne
                } );
            } else if( arg.Type == ArgumentType.List ) {
                cmd.Options.Add( new Option<string[]>( GetOptionName(arg), [.. GetOptionAliases(arg)] ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.ZeroOrMore
                } );
            } else {
                cmd.Options.Add( new Option<bool>( GetOptionName(arg), [.. GetOptionAliases(arg)] ) {
                    Description = arg.Description,
                    Arity = ArgumentArity.ZeroOrOne
                } );
            }
        }

        //This should be part of some sort of global configuration
        //service. Something that generates this list and maps them to an
        //IConfiguration for consumption in transients.
        if( definition.Handler is not null ) {
            cmd.Options.Add( new Option<string[]>( "--output-format", ["-of"] ) {
                Description = "Output format [text|json]", //TODO: Generate
                Arity = ArgumentArity.ZeroOrMore,
                AllowMultipleArgumentsPerToken = true
            } );
        }

        foreach( var subrouting in definition.SubCommands ?? [] ) {
            cmd.Subcommands.Add( CreateCommand( subrouting, false ) );
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
                        object? argValue = null;
                        try {
                            argValue = parsedArgs.GetValue<object?>( paramInfo.Name! );
                        } catch( ArgumentException ) {
                            argValue = parsedArgs.GetValue<object?>( GetOptionName(paramInfo.Name!) );
                        }

                        bool isMissing = true;
                        (isMissing, argValue) = ProcessArgument(
                            parameter: paramInfo,
                            definition: definition.ArgumentDefinitions.First( arg => arg.Name == paramInfo.Name ),
                            suppliedValue: argValue );
                        
                        paramList.Add(argValue);
                        if( isMissing ) missingParams.Add( paramInfo.Name! );
                    }
                }
                if( missingParams.Count != 0 ) throw new MissingArgumentsException( missingParams );

                var results = definition.Handler.Method.Invoke( definition.Handler.Target, [.. paramList ] );

                var outputFormat = parsedArgs.GetValue<string[]>("--output-format") ?? [];
                if( results is not null ) OutputResults(results, outputFormat);
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

    public void OutputResults( object results, string[] outputFormat ) {
        var format = outputFormat.Length == 0 ? "text" : outputFormat[0];

        //We should probably write the failure to STD::Error
        IFormattingService formattingService = 
            serviceProvider.GetKeyedService<IFormattingService>( format )
            ?? serviceProvider.GetRequiredKeyedService<IFormattingService>( "text" );

        formattingService.Configure( outputFormat.Skip(1) );

        if( results.GetType().IsArray || results.GetType().IsAssignableTo( typeof( IEnumerable ) ) ) {
            foreach( var item in results as IEnumerable<object> ?? [] ) {
                Console.WriteLine( formattingService.Print(item) );
            }
        } else {
            Console.WriteLine( formattingService.Print(results) );
        }
    }

    private static string GetOptionName( ArgumentDefinition definition )
        => GetOptionName(definition.Name);

    private static string GetOptionName( string name ) {
        name = ToKebabCase(name);
        return name.Length == 1 ? "-" + name : "--" + name;
    }

    private static IEnumerable<string> GetOptionAliases( ArgumentDefinition definition ) {
        foreach( var alias in definition.Aliases ) {
            var name = ToKebabCase(alias);
            yield return name.Length == 1 ? "-" + name : "--" + name;
        }
    }

    private static string ToKebabCase( string value ) {
        var strBuilder = new StringBuilder(value.Length);
        
        byte previousState = 0; //0 = First Character, 1 = Uppercase, 2 = Lowercase
        foreach( var c in value ) {
            if( char.IsUpper(c) ) {
                if( previousState == 2 ) strBuilder.Append( "-" + char.ToLower(c) );
                else strBuilder.Append( char.ToLower(c) );
                previousState = 1;
            } else {
                strBuilder.Append(c);
                previousState = 2;
            }
        }
        return strBuilder.ToString();
    }
}