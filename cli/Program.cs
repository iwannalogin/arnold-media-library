using arnold;
using arnold.Utilities;



var mainRouting = new List<CommandRouter>([
    CommandProcessor.FallbackRouter,
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
        Handler: Tag.Entry,
        Aliases: ["tag", "tags"]
    ),
    Configure.EntryRouter,
    Test.EntryRouter
]);

var processor = new CommandProcessor(
    CommandRouters: mainRouting,
    HelpText: CommandProcessor.GenerateHelpText(
        command: [],
        description: "A general purpose library management application",
        routings: mainRouting ) );

return processor.Execute( args );