using arnold.Utilities;

namespace arnold.Routing;

public partial class Routings {
    public static CommandDefinition AddTagCommand = new(
        name: "Add", description: "Add tag files or directories",
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

    public static CommandDefinition TagHandler = new(
        name: nameof(TagHandler),
        description: "Tag files or directories",
        subCommands: [ AddTagCommand ]
    );
}