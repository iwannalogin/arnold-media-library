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
    ),
    new CommandRouter(
        Name: "monitor",
        Description: "Manage file system monitors",
        Handler: arnold.Monitor.Entry,
        Aliases: ["mon" ,"monitor", "monitors"]
    ),
    new CommandRouter(
        Name: "tag",
        Description: "Manage file tags",
        Handler: arnold.Tag.Entry,
        Aliases: ["tag", "tags"]
    ), new CommandRouter(
        Name: "config",
        Description: "Manage configuration",
        Handler: arnold.Configure.Entry,
        Aliases: ["config", "configure"]
    )
]);

var helpText = @$"
arnold-library {{command}}

Commands:
  library: Create, remove, or manage libraries
".Trim();

var processor = new CommandProcessor( mainRouting, helpText );
return processor.Execute( args );