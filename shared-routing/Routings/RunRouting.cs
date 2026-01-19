using arnold.Models;
using arnold.Utilities;

namespace arnold.Routing;

public static class RunRouting {
    public static CommandDefinition MonitorHandler = new(
        name: nameof(MonitorHandler),
        description: "Run file library monitors",
        handler: static( [FromServices] ArnoldManager arnold, string? library = null ) => {
            var targetLibraries = library is null
                ? arnold.GetLibraries().AsEnumerable()
                : [arnold.GetLibrary(library) ?? throw new InvalidOperationException($"Failed to get library \"{library}.\"")];
            
            var newFiles = new List<FileMetadata>();
            foreach( var targetLibrary in targetLibraries ) {
                newFiles.AddRange( arnold.RunMonitors(targetLibrary) );
            }
            return newFiles;
        }
    );
}