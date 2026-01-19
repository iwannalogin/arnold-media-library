using arnold.Models;
using arnold.Utilities;

namespace arnold.Routing;

public static class AddRouting {
    public static CommandDefinition LibraryHandler = new(
        nameof(LibraryHandler),
        description: "Add library to database",
        handler: static ( [FromServices] ArnoldManager arnold, string name, string description )
            => arnold.AddLibrary( name, description )
    );

    public static CommandDefinition MonitorHandler = new (
        nameof(Monitor),
        description: "Attach monitor to library",
        handler: AddMonitor
    );
    private static FileMonitor AddMonitor( [FromServices] ArnoldManager arnold, string library, string name, string directory, string rule = ".*", bool recurse = false, bool exclusion = false ) {
        var fileLibrary = arnold.GetLibrary(library) ?? throw new InvalidOperationException($"Failed to get library \"{library}.\"");
        return arnold.AddMonitor(fileLibrary, name, directory, rule, recurse, !exclusion );
    }

    public static CommandDefinition TagHandler = new(
        nameof(TagHandler),
        description: "Add tags to files or directories",
        handler: static (
            [FromServices] ArnoldManager arnold,
            string library,
            string[]? tags = null,
            string[]? paths = null ) => {
                if( paths is null || paths.Length == 0 ) throw new InvalidOperationException("No paths provided");
                if( tags is null || tags.Length == 0 ) throw new InvalidOperationException("No tags provided");
                
                var fileLibrary = arnold.GetLibrary(library)
                    ?? throw new InvalidOperationException($"Failed to find library \"{library}.\"");

                var fileNames = paths.SelectMany( path => {
                    var pathAttr = File.GetAttributes(path);
                    if( pathAttr.HasFlag( FileAttributes.Directory ) ) {
                        return Directory.GetFiles( path, "*", SearchOption.AllDirectories );
                    }

                    return [path];
                }).Select( path => path.ToLower() );

                var files = arnold
                    .GetFiles(fileLibrary)
                    .Where( file => fileNames.Contains( file.Name.ToLower() ) );

                foreach( var file in files ) {
                    file.AddTags( tags );
                }
                arnold.UpdateFiles(files);
            }
    );
}