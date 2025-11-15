using arnold.Utilities;

namespace arnold.Routing;

public static class RootRouting {
    public static CommandDefinition RootHandler = new(
        name: nameof(RootHandler),
        description: string.Empty,
        subCommands: [ LibraryRouting.LibraryHandler, Routings.TagHandler ]
    );
}