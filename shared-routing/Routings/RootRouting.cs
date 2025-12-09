using arnold.Managers;
using arnold.Utilities;

namespace arnold.Routing;

public static class RootRouting {
    public static CommandDefinition UpdateHandler = new(
        name: nameof(UpdateHandler),
        description: "Update files in library",
        handler: ( [FromServices] LibraryManager libraryManager, string library ) => {
            return libraryManager.UpdateMetadata( library );
        }
    );

    public static CommandDefinition CleanHandler = new (
        nameof(CleanHandler),
        description: "Cleanup abandoned files in library",
        handler: LibraryRouting.CleanCommand.Handler
    );

    public static CommandDefinition AddTagHandler = new(
        name: "tag",
        description: "Add tag to file",
        handler: Routings.AddTagCommand.Handler
    );

    public static CommandDefinition AddLibraryHandler = new(
        name: "library",
        description: "Add library to database",
        handler: LibraryRouting.CreateCommand.Handler
    );

    public static CommandDefinition AddMonitorHandler = new(
        name: "monitor",
        description: "Add monitor to database",
        handler: ([FromServices] LibraryManager libraryManager, string library, string name, string directory, string rule, bool exclusionRule = false, bool recurse = false ) => {
            return libraryManager.AddMonitor( library, name, directory, rule, !exclusionRule, recurse );
        }
    );

    public static CommandDefinition AddHandler = new(
        nameof(AddHandler),
        description: "Add new entries to database",
        subCommands: [ AddTagHandler, AddLibraryHandler, AddMonitorHandler ]
    );

    public static CommandDefinition ListLibraryHandler = new(
        name: "libraries",
        description: "List libraries",
        handler: LibraryRouting.ListCommand.Handler
    );

    public static CommandDefinition ListMonitorHandler = new(
        name: "monitors",
        description: "List monitors for library",
        handler: ( [FromServices] LibraryManager libraryManager, string library ) => {
            var fileLibrary = libraryManager.GetLibrary(library);
            return fileLibrary?.Monitors;
        }
    );

    public static CommandDefinition ListHandler = new (
        name: "list",
        description: "List entries in database",
        subCommands: [ ListLibraryHandler, ListMonitorHandler ]
    );

    public static CommandDefinition SetAttributeHandler = new (
        name: "attribute",
        description: "Set attribute for file",
        handler: ( [FromServices] MetaManager metaManager, string library, string attribute, string value ) => {
            //var meta = libraryManager.GetMetadata
        }
    );

    public static CommandDefinition DefineAttributeHandler = new (
        name: "define-attribute",
        description: "Define an attribute",
        handler: (
            [FromServices] LibraryManager libraryManager,
            string name,
            string description = "No description" ) => {
                return libraryManager.DefineAttribute( name, description );
            }
    );

    public static CommandDefinition RootHandler = new(
        name: nameof(RootHandler),
        description: string.Empty,
        subCommands: [
            LibraryRouting.LibraryHandler,
            Routings.TagHandler,
            MetaRouting.MetaHandler,
            UpdateHandler,
            CleanHandler,
            AddHandler,
            ListHandler
        ]
    );
}