using arnold.Utilities;

namespace arnold.Routing;

public static class RootRouting {
    public static CommandDefinition UpdateHandler = new(
        name: nameof(UpdateHandler),
        description: "Update files in library",
        handler: ( [FromServices] ArnoldManager arnold, string library ) => {
            var fileLibrary = arnold.GetLibrary(library) ?? throw new InvalidOperationException($"Unable to find library \"{library}.\"");
            return arnold.RunMonitors( fileLibrary );
        }
    );

    public static CommandDefinition CleanHandler = new (
        nameof(CleanHandler),
        description: "Cleanup abandoned files in library",
        handler: LibraryRouting.CleanCommand.Handler
    );

    public static CommandDefinition AddHandler = new(
        nameof(AddHandler),
        description: "Add new entries to database",
        subCommands: [ AddRouting.TagHandler, AddRouting.LibraryHandler, AddRouting.MonitorHandler ]
    );

    public static CommandDefinition EditHandler = new(
        nameof(EditHandler),
        description: "Edit entries in the database",
        subCommands: [ EditRouting.LibraryHandler ]
    );

    public static CommandDefinition RemoveHandler = new(
        nameof(RemoveHandler),
        description: "Remove entries from the database",
        subCommands: [ RemoveRouting.MonitorHandler ]
    );

    public static CommandDefinition RunHandler = new(
        nameof(RunHandler),
        description: "Run actions",
        subCommands: [ RunRouting.MonitorHandler ]
    );

    public static CommandDefinition ListLibraryHandler = new(
        name: "libraries",
        description: "List libraries",
        handler: LibraryRouting.ListCommand.Handler
    );

    public static CommandDefinition ListMonitorHandler = new(
        name: "monitors",
        description: "List monitors for library",
        handler: ( [FromServices] ArnoldManager arnold, string library ) => {
            var fileLibrary = arnold.GetLibrary(library);
            return fileLibrary?.Monitors;
        }
    );

    public static CommandDefinition ListHandler = new (
        name: "list",
        description: "List entries in database",
        subCommands: [ ListLibraryHandler, ListMonitorHandler ]
    );

    //public static CommandDefinition SetAttributeHandler = new (
    //    name: "attribute",
    //    description: "Set attribute for file",
    //    handler: ( [FromServices] MetaManager metaManager, string library, string attribute, string value ) => {
    //        //var meta = libraryManager.GetMetadata
    //    }
    //);

    public static CommandDefinition DefineAttributeHandler = new (
        name: "define-attribute",
        description: "Define an attribute",
        handler: (
            [FromServices] ArnoldManager arnold,
            string name,
            string description = "No description" ) => {
                return arnold.AddAttributeDefinition( name, description );
            }
    );

    public static CommandDefinition UpdateProvidersHandler = new(
        name: "update-providers",
        description: "Run all providers",
        handler: ( [FromServices] ArnoldManager arnold, string library ) => {
            arnold.RunProviders( arnold.GetLibrary(library)! );
        }
    );

    public static CommandDefinition RenameHandler = new(
        name: "rename",
        description: "Rename file",
        handler: ( [FromServices] ArnoldManager arnold, string oldFile, string newFile  ) => {
            var newDirectory = Path.GetDirectoryName(newFile)!;
            if( !Directory.Exists(newDirectory) ) Directory.CreateDirectory(newDirectory);

            File.Move( oldFile, newFile );
            var files = arnold.GetFiles().Where( file => file.Name.ToLower() == oldFile.ToLower() );
            foreach( var file in files ) {
                file.Name = newFile;
                file.Label = Path.GetFileName(newFile);
            }
            arnold.UpdateFiles(files);
        }
    );

    public static CommandDefinition RetargetDirectoryHandler = new(
        name: "retarget-directory",
        description: "Retarget full directory",
        handler: ( [FromServices] ArnoldManager arnold, string oldDirectory, string newDirectory ) => {
            oldDirectory = oldDirectory.ToLower();

            if( !oldDirectory.EndsWith("/") ) oldDirectory += "/";
            if( !newDirectory.EndsWith("/") ) newDirectory += "/";
            
            var files = arnold.GetFiles()
                .Where( file => file.Name.ToLower().StartsWith( oldDirectory ) );
            foreach( var file in files ) {
                file.Name = newDirectory + file.Name[oldDirectory.Length..];
            }

            arnold.UpdateFiles(files);
        }
    );

    public static CommandDefinition RootHandler = new(
        name: nameof(RootHandler),
        description: string.Empty,
        subCommands: [
            //LibraryRouting.LibraryHandler,
            //MetaRouting.MetaHandler,
            //UpdateHandler,
            //CleanHandler,
            AddHandler,
            EditHandler,
            RemoveHandler,
            RunHandler
            //ListHandler,
            //DefineAttributeHandler,
            //UpdateProvidersHandler,
            //RenameHandler,
            //RetargetDirectoryHandler
        ]
    );
}