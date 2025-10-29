using arnold.Utilities;

namespace arnold.Routing;

public static class RootRouting {
    public static CommandDefinition LibraryHandler = new(
        name: nameof(LibraryHandler),
        description: "Manage libraries",
        subCommands: [ LibraryRouting.ListCommand ]
    );

    public static CommandDefinition RootHandler = new(
        name: nameof(RootHandler),
        description: string.Empty,
        subCommands: [ LibraryHandler ]
    );
}