using arnold.Managers;
using arnold.Utilities;

namespace arnold.Routing;

public partial class Routings {
    public static CommandDefinition AddTagCommand = new(
        name: "Add", description: "Add tag files or directories",
        handler: static (
            [FromServices] LibraryManager libraryManager,
            [FromServices] MetaManager metaManager,
            string library,
            string[]? tags = null,
            string[]? paths = null ) => {
                var fileLibrary = libraryManager.GetLibrary(library);
                if( fileLibrary is null ) return null;

                metaManager.AddTags( fileLibrary, paths ?? [], tags ?? [] );
                return string.Empty;
            }
    );

    public static CommandDefinition TagHandler = new(
        name: nameof(TagHandler),
        description: "Tag files or directories",
        subCommands: [ AddTagCommand ]
    );
}