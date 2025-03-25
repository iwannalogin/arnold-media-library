using arnold.Services;

namespace arnold;

class Configure {
    public static int Entry( string[] args ) {
        var configureRouting = new List<CommandRouter>([
            CommandProcessor.FallbackRouter,
            new CommandRouter(
                Name: "Info",
                Description: "Display configuration information",
                Handler: InfoHandler,
                Aliases: [ "info" ]
            )
        ]);

        var processor = new CommandProcessor(configureRouting, "HELP TEXT");
        return processor.Execute(args);
    }

    public static int InfoHandler( string[] args ) {
        var arguments = ArgumentProcessor.Process(
            args: args,
            argumentMap: []
        );

        using var dataService = Provider.GetRequiredService<DataService>();
        Console.WriteLine($"Database Path: {dataService.DbPath}");
        return 0;
    }
}