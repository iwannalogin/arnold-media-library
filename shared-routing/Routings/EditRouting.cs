using arnold.Utilities;

namespace arnold.Routing;

public static class EditRouting {
    public static CommandDefinition LibraryHandler = new (
        nameof(LibraryHandler),
        description: "Edit library properties",
        handler: static ( [FromServices] ArnoldManager arnold, string library, string? name = null, string? description = null ) => {
            var fileLibrary = arnold.GetLibrary(library) ?? throw new InvalidOperationException($"Failed to find library \"{library}.\"");
            if( name is not null ) fileLibrary.Name = name;
            if( description is not null ) fileLibrary.Description = description;

            arnold.UpdateLibrary(fileLibrary);
            return fileLibrary;
        }
    );
}