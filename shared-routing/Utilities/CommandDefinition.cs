using System.Reflection;

namespace arnold.Utilities;

public class CommandDefinition {
    public string Name { get; init; }
    public string Description { get; init; }
    public Delegate? Handler { get; init; }
    public IEnumerable<CommandDefinition> SubCommands { get; init; }
    private Lazy<IEnumerable<ArgumentDefinition>> _argumentDefinitions;
    public IEnumerable<ArgumentDefinition> ArgumentDefinitions => _argumentDefinitions.Value;

    public CommandDefinition( string name, string description, Delegate? handler = null, IEnumerable<CommandDefinition>? subCommands = null ) {
        Name = name.Replace("Handler", "");
        Description = description;
        Handler = handler;
        SubCommands = subCommands ?? [];
        _argumentDefinitions = Handler is null
            ? new Lazy<IEnumerable<ArgumentDefinition>>( [] )
            : new Lazy<IEnumerable<ArgumentDefinition>>( () => GetArgumentDefinitions(Handler) );
    }

    private static IEnumerable<ArgumentDefinition> GetArgumentDefinitions( Delegate handler ) {
        foreach( var param in handler.Method.GetParameters() ) {
            if( param.GetCustomAttribute<FromServicesAttribute>() is not null ) continue;

            yield return new ArgumentDefinition(param);
        }
    }
}