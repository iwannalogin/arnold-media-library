using arnold.Managers;
using arnold.Models;
using arnold.Utilities;

namespace arnold.Routing;

public static class LibraryRouting {
    public static CommandDefinition ListCommand = new(
        name: "list",
        description: "List existing libraries",
        handler: static ( [FromServices] LibraryManager libraryManager )
                => libraryManager.ListLibraries()
                    .AsNoTracking()
                    .Include( fl => fl.Files )
                    .Include( fl => fl.Monitors )
    );
}