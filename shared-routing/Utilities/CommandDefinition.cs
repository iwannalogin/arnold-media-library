using System.Reflection;

namespace arnold.Utilities;

public class CommandDefinition {
    public string Name { get; init; }
    public string Description { get; init; }
    public Delegate? Handler { get; init; }
    public IEnumerable<CommandDefinition> SubCommands { get; init; }
    
    private IEnumerable<ArgumentDefinition>? _argumentDefinitions;
    public IEnumerable<ArgumentDefinition> ArgumentDefinitions {
        get {
            _argumentDefinitions ??= Handler is null ? [] : GetArgumentDefinitions(Handler).ToList();
            return _argumentDefinitions;
        }
    }

    public CommandDefinition( string name, string description, Delegate? handler = null, IEnumerable<CommandDefinition>? subCommands = null ) {
        Name = name.Replace("Handler", "");
        Description = description;
        Handler = handler;
        SubCommands = subCommands ?? [];
    }

    private static IEnumerable<ArgumentDefinition> GetArgumentDefinitions( Delegate handler ) {
        foreach( var param in handler.Method.GetParameters() ) {
            if( param.GetCustomAttribute<FromServicesAttribute>() is not null ) continue;

            yield return new ArgumentDefinition(param);
        }
    }
}