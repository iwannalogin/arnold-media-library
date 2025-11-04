using System.Text;

namespace arnold.Utilities;

public class RoutingFactory {
    public void CreateEndpoints( WebApplication web, CommandDefinition definition, CommandDefinition[]? parentChain = null ) {
        var arguments = SortArguments( definition.ArgumentDefinitions ?? [] );

        if( definition.Handler is not null ) {
            var routeBuilder = new List<string>();
            foreach( var parentDefinition in (parentChain ?? []) ) {
                routeBuilder.Add(parentDefinition.Name);
            }
            routeBuilder.Add( definition.Name );
            routeBuilder.RemoveAt(0);

            web.MapGet( string.Join("/", routeBuilder), definition.Handler );
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
}