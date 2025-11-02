using arnold.Managers;
using arnold.Utilities;
using Microsoft.EntityFrameworkCore;

namespace arnold.Routing;

public static class LibraryRouting {

    public static CommandDefinition ListCommand = new(
        "list", "List existing libraries",
        handler: static ( [FromServices] LibraryManager libraryManager )
                => libraryManager.ListLibraries()
                    .AsNoTracking()
                    .Include( fl => fl.Files )
                    .Include( fl => fl.Monitors )
    );

    public static CommandDefinition CreateCommand = new(
        "create", "Create a new library",
        handler: static ( [FromServices] LibraryManager libraryManager, string name, string? description )
            => libraryManager.CreateLibrary( name, description )
    );

    public static CommandDefinition DeleteCommand = new(
        "delete", "Delete existing library",
        handler: static ( [FromServices] LibraryManager libraryManager, string name )
            => libraryManager.DeleteLibrary( name )
    );

    public static CommandDefinition InfoCommand = new(
        "info", "Get library info",
        handler: static ( [FromServices] LibraryManager libraryManager, string name )
            => libraryManager.GetLibrary( name )
    );

    public static CommandDefinition SearchCommand = new(
        name: "search", description: "Search libraries",
        handler: (
            [FromServices] LibraryManager libraryManager,
            string library,
            string[]? tags = null, string[]? excludeTags = null,
            SearchMode mode = SearchMode.All ) => {
            
            var fileLibrary = libraryManager.GetLibrary(library);
            if( fileLibrary is null ) return null;

            tags = [.. (tags ?? []).Select( tag => tag.ToLower() )];
            excludeTags = [.. (excludeTags ?? []).Select( tag => tag.ToLower() )];

            var searchQuery = libraryManager.ListMetadata(fileLibrary);
            if( tags.Length != 0 || excludeTags.Length != 0) {
                searchQuery = searchQuery.Include( meta => meta.Tags );
            }

            if( tags.Length != 0 && mode == SearchMode.All ) {
                searchQuery = searchQuery.Where( meta => tags.All( tag => meta.Tags.Any( mtag => mtag.Tag.ToLower() == tag ) ) );
            } else if( tags.Length != 0 && mode == SearchMode.Any ) {
                searchQuery = searchQuery.Where( meta => tags.Any( tag => meta.Tags.Any( mtag => mtag.Tag.ToLower() == tag ) ) );
            }

            if( excludeTags.Length != 0 && mode == SearchMode.All ) {
                searchQuery = searchQuery.Where( meta => excludeTags.All( tag => meta.Tags.Any( mtag => mtag.Tag.ToLower() == tag ) ) == false );
            } else if( excludeTags.Length != 0 && mode == SearchMode.Any ) {
                searchQuery = searchQuery.Where( meta => excludeTags.Any( tag => meta.Tags.Any( mtag => mtag.Tag.ToLower() == tag ) ) == false );
            }

            return searchQuery.ToList();
        }
    );

    public static CommandDefinition LibraryHandler = new(
        name: nameof(LibraryHandler),
        description: "Manage libraries",
        subCommands: [ ListCommand, CreateCommand, DeleteCommand, InfoCommand, SearchCommand ]
    );
}

public enum SearchMode : byte {
    All, Any
}