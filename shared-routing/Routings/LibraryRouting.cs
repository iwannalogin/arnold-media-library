using arnold.Utilities;
using Microsoft.EntityFrameworkCore;

namespace arnold.Routing;

public static class LibraryRouting {

    public static CommandDefinition ListCommand = new(
        "list", "List existing libraries",
        handler: static ( [FromServices] ArnoldManager arnold )
                => arnold.GetLibraries()
                    .AsNoTracking()
                    .Include( fl => fl.Files )
                    .Include( fl => fl.Monitors )
    );

    public static CommandDefinition CreateCommand = new(
        "create", "Create a new library",
        handler: static ( [FromServices] ArnoldManager arnold, string name, string description )
            => arnold.AddLibrary( name, description )
    );

    public static CommandDefinition DeleteCommand = new(
        "delete", "Delete existing library",
        handler: static ( [FromServices] ArnoldManager arnold, string name ) => {
            var library = arnold.GetLibrary(name);
            if( library is null ) return $"Library \"{name}\" does not exist.";
            arnold.DeleteLibrary(library);
            return $"Deleted \"{library.Name}.\"";
        }
    );

    public static CommandDefinition InfoCommand = new(
        "info", "Get library info",
        handler: static ( [FromServices] ArnoldManager arnold, string name )
            => arnold.GetLibrary( name )
    );

    public static CommandDefinition SearchCommand = new(
        name: "search", description: "Search libraries",
        handler: (
            [FromServices] ArnoldManager arnold,
            string library,
            string[]? tags = null, string[]? excludeTags = null, string[]? paths = null,
            SearchMode mode = SearchMode.All ) => {
            
            var fileLibrary = arnold.GetLibrary(library);
            if( fileLibrary is null ) return null;

            tags = [.. (tags ?? []).Select( tag => tag.ToLower() )];
            excludeTags = [.. (excludeTags ?? []).Select( tag => tag.ToLower() )];
            paths = [.. (paths ?? []).Select( path => path.ToLower() )];

            var searchQuery = arnold.GetFiles(fileLibrary);
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

            if( paths.Length != 0 && mode == SearchMode.All ) {
                searchQuery = searchQuery.Where( meta => paths.All( path => meta.Name.ToLower().Contains(path) ) );
            } else if( paths.Length != 0 && mode == SearchMode.Any ) {
                searchQuery = searchQuery.Where( meta => paths.Any( path => meta.Name.ToLower().Contains(path) ) );
            }

            return searchQuery.ToList();
        }
    );

    public static CommandDefinition CleanCommand = new(
        name: "clean", description: "Remove abandoned files from library",
        handler: static (
                [FromServices] ArnoldManager arnoldManager,
                string library ) => {
            var fileLibrary = arnoldManager.GetLibrary(library);
            if( fileLibrary is null ) return null;

            var abandonedMeta = arnoldManager
                .GetFiles(fileLibrary)
                .AsEnumerable()
                .Where( file => !File.Exists(file.Name) );
            
            arnoldManager.DeleteFiles(abandonedMeta);
            return abandonedMeta;
        }
    );

    public static CommandDefinition ListTagsCommand = new(
        name: "list-tags", description: "List all tags used in library",
        handler: static ( [FromServices] ArnoldManager arnold, string library ) => {
            var fileLibrary = arnold.GetLibrary(library);
            if( fileLibrary is null ) return null;

            return arnold.GetFiles(fileLibrary)
                .SelectMany( meta => meta.Tags )
                .ToList()
                .Select( tag => tag.Tag )
                .GroupBy( tag => tag )
                .Select( group => new TagSummary( group.Key, group.Count() ) );
        }
    );

    public static CommandDefinition LibraryHandler = new(
        name: nameof(LibraryHandler),
        description: "Manage libraries",
        subCommands: [ ListCommand, CreateCommand, DeleteCommand, InfoCommand, SearchCommand, CleanCommand, ListTagsCommand ]
    );
}

public enum SearchMode : byte {
    All, Any
}

public class TagSummary( string tag, int count ) {
    public string Tag => tag;
    public int Count => count;

    public override string ToString()
        => $"{tag}: {count}";
}