using arnold.Utilities;

namespace arnold.Routing;

public static class MetaRouting {
    public static CommandDefinition TagsCommand = new (
        name: "tags", description: "List tags for file entry",
        handler: ([FromServices] ArnoldManager arnold, string library, string file ) => {
            var fileLibrary = arnold.GetLibrary(library);
            if( fileLibrary is null ) throw new InvalidOperationException("Failed to find library");
            var tags = arnold.GetFile(fileLibrary, file)?.Tags;
            if( tags is null ) throw new InvalidOperationException("Failed to find metadata");
            return tags.Select( tag => tag.Tag );
        }
    );

    public static CommandDefinition MetaHandler = new(
        name: nameof(MetaHandler),
        description: "Managa metadata",
        subCommands: [ TagsCommand ]
    );
}