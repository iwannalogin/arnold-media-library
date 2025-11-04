using System.Reflection;

namespace arnold.Utilities;

public class RoutingLink( string route, string method, CommandDefinition command ) {
    public string Route { get; set; } = route;
    protected string[]? _pathSegments = null;
    public string[] PathSegments {
        get {
            _pathSegments ??= [.. Route.Split('/').Select( segment => segment.ToLower() )];
            return _pathSegments;
        }
    }
    public string Method { get; set; } = method;
    public string[] RequiredArguments { get; set; } = [
        
    ];
    public CommandDefinition Command { get; set; } = command;

    public bool IsMatch( HttpRequest request ) {
        if( Method.Equals( request.Method, StringComparison.InvariantCultureIgnoreCase ) == false ) {
            return false;
        }

        var requestSegments = request.Path.ToString().TrimStart('/').Split('/');
        if( requestSegments.Length < (PathSegments.Length + RequiredArguments.Length) ) {
            return false;
        }

        for( int i = 0; i < PathSegments.Length; i++ ) {
            if( !PathSegments[i].Equals( requestSegments[i], StringComparison.InvariantCultureIgnoreCase ) ) {
                return false;
            }
        }

        return true;
    }

    public async Task Invoke( HttpContext context, IServiceProvider serviceProvider ) {
        var requestSegments = context.Request.Path.ToString().TrimStart('/').Split();
        var paramList = new List<object?>();
        var serviceList = new List<object>();
        
        foreach( var paramInfo in Command.Handler!.Method.GetParameters() ) {
            if( paramInfo.GetCustomAttribute<FromServicesAttribute>() is not null ) {
                var service = serviceProvider.GetRequiredService( paramInfo.ParameterType );
                paramList.Add(service);
                serviceList.Add(service);
            } else {
                object? argValue = null;
                var requiredIndex = IndexOf( RequiredArguments, paramInfo.Name! );
                if( requiredIndex >= 0 ) {
                    argValue = requestSegments[requiredIndex + PathSegments.Length];
                } else if( context.Request.Query.ContainsKey(paramInfo.Name!) ) {
                    argValue = context.Request.Query[paramInfo.Name!];
                }
                paramList.Add(argValue);
            }
        }

        
        if( Command.Handler.Method.ReturnType == typeof(void) ) {
            Command.Handler.Method.Invoke( Command.Handler.Target, [.. paramList] );
        } else if( Command.Handler.Method.ReturnType == typeof(Task) ) {
            var task = Command.Handler.Method.Invoke( Command.Handler.Target, [.. paramList] ) as Task;
            await task!;
        } else {
            var result = Command.Handler.Method.Invoke( Command.Handler.Target, [.. paramList] );
            if( result is Task<object> resultTask ) result = await resultTask;
            if( result is not null ) {
                if( result.GetType().IsPrimitive ) {
                    await context.Response.WriteAsync( result?.ToString() ?? string.Empty );
                } else {
                    var jsonResult = new JsonHelper().Print(result);
                    await context.Response.WriteAsync( jsonResult );
                }
            }
        }
        context.Response.StatusCode = 200;
    }

    protected int IndexOf( string[] array, string value ) {
        for( int i = 0; i < array.Length; i++ ) {
            if( value.Equals( array[i], StringComparison.InvariantCultureIgnoreCase ) ) {
                return i;
            }
        }
        return -1;
    }
};

public class RoutingChain( CommandDefinition rootCommand ) {
    public CommandDefinition Root => rootCommand;
    public IEnumerable<RoutingLink> RoutingLinks = CreateRoutingLinks(rootCommand);

    protected static IEnumerable<RoutingLink> CreateRoutingLinks( CommandDefinition rootCommand ) {
        var commandStack = new Stack<CommandDefinition>();
        var routingLinks = new List<RoutingLink>();
        var activeCommand = rootCommand;
        do {
            var method = InferRequestMethod(activeCommand);
            if( !string.IsNullOrWhiteSpace(method) ) {
                routingLinks.Add( new(
                    route: string.Join("/", [.. commandStack.Reverse().Skip(1).Select( cmd => cmd.Name.ToLower() ), activeCommand.Name.ToLower()] ),
                    method: method,
                    command: activeCommand
                ) );
            }

            if( activeCommand.SubCommands.Any() ) {
                commandStack.Push(activeCommand);
                activeCommand = activeCommand.SubCommands.First();
            } else {
                CommandDefinition? nextCommand = null;
                while( nextCommand is null && commandStack.Count > 0 ) {
                    var parentCommand = commandStack.Pop();
                    nextCommand = GetNextCommand( parentCommand, activeCommand );
                    activeCommand = parentCommand;
                }

                if( nextCommand is null ) {
                    activeCommand = rootCommand;
                } else {
                    commandStack.Push( activeCommand );
                    activeCommand = nextCommand;
                }
            }
        } while( activeCommand != rootCommand );
        return routingLinks;
    }

    protected static CommandDefinition? GetNextCommand( CommandDefinition parentCommand, CommandDefinition activeCommand ) {
        var returnNext = false;
        foreach( var command in parentCommand.SubCommands ) {
            if( returnNext ) return command;
            returnNext = command == activeCommand;
        }
        return null;
    }

    public static IEnumerable<string> GetRequiredArguments( CommandDefinition definition ) {
        foreach( var argDef in definition.ArgumentDefinitions ) {
            if( argDef.Required && argDef.Type == ArgumentType.Value ) {
                yield return argDef.Name;
            }
        }
    }

    public static string InferRequestMethod( CommandDefinition command ) {
        if( command.Handler is null ) return string.Empty;
        else if( IsPut( command.Handler ) ) return "PUT";
        else if( IsGet( command.Handler ) ) return "GET";
        else return "POST";
    }

    private static bool IsPut( Delegate requestDelegate ) {
        var returnType = requestDelegate.Method.ReturnType;
        return returnType == typeof(void)
            || returnType == typeof(Task);
    }

    private static bool IsGet( Delegate requestDelegate ) {
        foreach( var param in requestDelegate.Method.GetParameters() ) {
            if( !ArgumentDefinition.IsRequired(param) || param.GetCustomAttribute<FromServicesAttribute>() is not null ) {
                continue;
            } else if( !param.ParameterType.IsPrimitive ) {
                return false;
            }
        }
        return true;
    }
}