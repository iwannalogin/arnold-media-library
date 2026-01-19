using arnold.Utilities;

namespace arnold.Routing;

public static class RemoveRouting {
    public static CommandDefinition MonitorHandler = new(
        nameof(MonitorHandler),
        description: "Remove monitors from library",
        handler: static ( [FromServices] ArnoldManager arnold, string library, string monitor ) => {
            var fileLibrary = arnold.GetLibrary(library) ?? throw new InvalidOperationException($"Failed to find library \"{library}.\"");
            var fileMonitor = arnold.GetMonitor(fileLibrary, monitor ) ?? throw new InvalidOperationException($"Failed to find monitor \"{monitor}.\"");

            arnold.DeleteMonitor(fileMonitor);
        }
    );
}