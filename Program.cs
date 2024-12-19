using arnold;

var mainRouting = new List<CommandRouter>([
    new CommandRouter(
        Name: "fallback",
        Description: "FAILS",
        Handler: args => {
            Console.WriteLine("Welcome to Arnold's Media Library. Use the --help flag for instructions.");
            return 0;
        },
        Aliases: Array.Empty<string>()
    ),
    new CommandRouter(
        Name: "library",
        Description: "Manage libraries",
        Handler: Library.Entry,
        Aliases: ["lib", "libs", "library", "libraries"]
    )
]);

var helpText = @$"
arnold-library {{command}}

Commands:
  library: Create, remove, or manage libraries
".Trim();

var processor = new CommandProcessor( mainRouting, helpText );
return processor.Execute( args );